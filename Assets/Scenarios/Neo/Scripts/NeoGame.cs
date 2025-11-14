using System;
using System.Collections;
using System.Data;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class NeoGame : SplitGame
{
	private static readonly int Swipe = Shader.PropertyToID("Swipe");

	[Space]
	public GameObject gameGroup;
	public GameObject configWindow;
	public Transform[] satellites;
	public NeoModelRotate[] grabRotates;

	[Space] 
	public Transform sunRot;
	public Transform earthRot;
	public Vector2 phaseRots;

	[Space] 
	public FlightPath[] paths;

	public Transform flightTrans;
	public GameObject[] flightSatellites;

	[Space] 
	public Camera cam;
	public NeoUI ui;
	public TextAsset yearData;

	[Space] 
	public Transform topPivot;
	public Transform bottomPivot;
	
	[Serializable]
	public class FlightPath
	{
		public Transform root;
		public Vector2 range;
		public SpriteRenderer sR;
		private Material mat;
		public GameObject selectionObjects;
		public SpriteRenderer selectionRend;
		public Sprite[] selectionSprites;

		public Vector3 Animate(Transform sat, float time, Camera cam)
		{
			Vector3 r = root.position;

			float angle = Mathf.Lerp(range.x, range.y, time);
			Vector3 pos = r + Quaternion.AngleAxis(angle, Vector3.back) * Vector3.up * root.localScale.x;

			sat.position = pos;
			sat.rotation = Quaternion.LookRotation(pos - cam.transform.position, pos - r) *
			               Quaternion.AngleAxis(Time.unscaledTime * 50, Vector3.up);

			pos = cam.WorldToViewportPoint(pos);
			mat.SetFloat(Swipe, pos.x);

			return pos;
		}

		public void Init()
		{
			mat = Instantiate(sR.material);
			sR.material = mat;
		}

		public void Reset()
		{
			if(mat)
				mat.SetFloat(Swipe, 0);
		}

		public void Select(bool doit)
		{
			selectionObjects.SetActive(doit);
			selectionRend.sprite = selectionSprites[doit ? 1 : 0];
		}
	}
	
	
	private float slideGoal;
	private readonly FloatForce sForce = new FloatForce(200).SetSpeed(300).SetDamp(40);
	private readonly FloatForce pForce = new FloatForce(200).SetSpeed(150).SetDamp(30);
	private int state;

	public float[] lifeTime;

	public const float MaxLifetime = 150;

	
	private void Start()
	{
		SetActive(false);
		
		for (int i = 0; i < 3; i++)
			paths[i].Init();
		
		//ParseData();
	}

	public void ParseData()
	{
		string[] lines = yearData.text.Split('\n');
		int count = lines.Length;
		lifeTime = new float[count];
		for (int i = 0; i < count; i++)
			lifeTime[i] = float.Parse(lines[i]);

		return;
		float maxLife = 0;
		for (int s = 0; s < 3; s++)
		for (int o = 0; o < 3; o++)
		for (int a = 0; a < 3; a++)
		for (int b = 0; b < 3; b++)
		for (int c = 0; c < 3; c++)
		{
			Vector3 values = SatValues(s, o, a, b, c);
			float life = Mathf.Min(values.x, Mathf.Min(values.y, values.z));
			maxLife = Mathf.Max(maxLife, life);
		}
		
		Debug.Log("Max Life Time: " + maxLife);
	}

	
	public override SplitGame GameStart(bool activeWindow = true)
    {
	    base.GameStart(activeWindow);
	    SetActive(true);
	    return this;
    }
    
	
    public override void GameStop()
    {
	    base.GameStop();
	    SetActive(false);
    }

    
    private void SetActive(bool value)
    {
	    gameGroup.SetActive(value);
    }
    
    
    public void ActivateConfigWindow(bool active)
    {
	    configWindow.SetActive(active);

	    if (active)
	    {
		    slideGoal = NeoUI.SAT;
		    sForce.SetValue(NeoUI.SAT);
		    sForce.SetForce(0);
	    }
	    
	    for (int i = 0; i < 3; i++)
		    grabRotates[i].ShowWindow();
    }

    
    public void LeftRight(bool left)
    {
	    slideGoal += left ? -1 : 1;
    }

    
    private void LateUpdate()
    {
	    float l = pForce.Update(state == 1 ? 1 : 0, Time.deltaTime);
	    sunRot.localRotation = Quaternion.AngleAxis(Mathf.LerpUnclamped(phaseRots.x, 0, l), Vector3.right);
	    earthRot.localRotation = Quaternion.AngleAxis(Mathf.LerpUnclamped(phaseRots.y, 0, l), Vector3.right);
	    
	    float slide = sForce.Update(slideGoal, Time.deltaTime);
	    for (int i = 0; i < 3; i++)
	    {
		    float pLerp = Mathf.Repeat(-i + slide + 1, 3) - 1;
		    satellites[i].localPosition = Vector3.right * (pLerp * -9);
	    }
    }


    public void SetPhase(int state)
    {
	    this.state = state;

	    flightTrans.position = Vector3.down * 10000;
	    for (int i = 0; i < 3; i++)
		    paths[i].Reset();
    }


    private Vector3 SatValues(int sat, int orb, int pu, int pr, int sp)
    {
	    const int MINSUN = 9;
	    int index = sat * 54 + orb * 18 + MINSUN;
	    float puTime = lifeTime[index     + Mix[pu]];
	    float prTime = lifeTime[index + 3 + Mix[pr]];
	    float spTime = lifeTime[index + 6 + Mix[sp]];
	    
	    return new Vector3(puTime, prTime, spTime);
    }

    private readonly int[] Mix = { 0, 2, 1 };
    
    public Vector3 StartFlight()
    {
	    for (int i = 0; i < 3; i++)
		    paths[i].Reset();
	    
	    for (int i = 0; i < 3; i++)
		    flightSatellites[i].SetActive(i == NeoUI.SAT);
	    
	    Vector3 values = SatValues(NeoUI.SAT, NeoUI.ORB, NeoUI.PU, NeoUI.PR, NeoUI.SP);

	    float life = Mathf.Min(values.x, Mathf.Min(values.y, values.z));
	    
	    Debug.Log("PU: " + values.x.ToString("F5"));
	    Debug.Log("PR: " + values.y.ToString("F5"));
	    Debug.Log("SP: " + values.z.ToString("F5"));
	    Debug.Log(life.ToString("F5"));
	    
	    ui.FlightStart();
	    
	    StartCoroutine(FlightAnim(NeoUI.ORB, life));
	    return values / MaxLifetime;
    }


    private IEnumerator FlightAnim(int path, float life)
    {
	    float range = .6f + .4f * (life / MaxLifetime);
	    float speed = 1f / range;
	    
	    float t = 0;
	    while (true)
	    {
		    t += Time.deltaTime * .075f * speed;
		    
		    Vector3 pos = paths[path].Animate(flightTrans, t * range, cam);

		    if (t >= 1)
		    {
			    ui.FlightUpdate( Mathf.RoundToInt(life), life / MaxLifetime, pos);
			    break;
		    }
		   
			ui.FlightUpdate( Mathf.RoundToInt(t * life), t * life/ MaxLifetime, pos);

		    yield return null;
	    }

	    ui.FlightOver(Mathf.CeilToInt(life));
    }


    public Vector3 PivotViewPos(bool top)
    {
	    return cam.WorldToViewportPoint(top? topPivot.position : bottomPivot.position);
    }

    public void SelectOrbit(int id)
    {
	    //Debug.Log(id);
	    for (int i = 0; i < 3; i++)
		    paths[i].Select(i == id);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(NeoGame))]
public class NeoGameEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (GUILayout.Button("Parse"))
		{
			(target as NeoGame).ParseData();
			EditorUtility.SetDirty(target);
		}
	}
}
#endif