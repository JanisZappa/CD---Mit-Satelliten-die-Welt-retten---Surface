using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthMenu : Singleton<EarthMenu>
{
    public Camera cam;
    public LayerMask mask;
    private Collider coll;
    private static readonly int Angle = Shader.PropertyToID("Angle");
    
    private bool grabbing;
    private Vector3 grabPoint;
    
    private Transform trans;
    private float angle = 200;
    private readonly FloatForce angleForce = new FloatForce(300).SetSpeed(40).SetDamp(17);

    public MeshRenderer earthRenderer;
    [Header("Check")]
    public float toShader;

    private Material mat;

    public GameObject devText;
    public static bool DevMode;
    
    private Transform markers;
    
    private int markerCount;
    private SphereCollider[] markerColl;
    private float[] markerA;
    private Dictionary<string, int> map;
    
    private NewUI ui;

    
    private void Start()
    {
        mat = Instantiate(earthRenderer.material);
        earthRenderer.material = mat;
        
        coll = GetComponent<Collider>();
        trans = transform;
        
        markers = trans.GetChild(0);
        markerCount = markers.childCount;
        markerColl = new SphereCollider[markerCount];
        markerA = new float[markerCount];
        
        map = new Dictionary<string, int>();
        for (int i = 0; i < markerCount; i++)
        {
            Transform t = markers.GetChild(i);
            t.localPosition = t.localPosition.normalized * (9 * .5f + .15f * .5f) * (t.gameObject.CompareTag("FreeMarker")? 1.5f : 1);
            
            SphereCollider c = t.GetComponent<SphereCollider>();
            c.radius = 2.4f;
            markerColl[i] = c;
            map.Add(c.name, i);
        }
        
        angleForce.SetValue(angle / 360f);
        
        ui = FindObjectOfType<NewUI>();

        if (SavedMarkers != markerCount)
        {
            SavedMarkers.Set(markerCount);

            for (int i = 0; i < markerCount; i++)
                SetMarkerAsHidden(i, false);
        }
        
        devText.SetActive(false);
    }
    

    private void Update()
    {
        if(!SplitAnim.EarthView)
            return;

        DevModeCodeCheck();
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, 10000, mask))
                if(hit.collider == coll)
                {
                    ChangeGrabbing(true);
                    grabPoint = hit.point.SetY(0).normalized;
                }
                else
                {
                    int hitId = map[hit.collider.name];
                    if (markerA[hitId] > .1f)
                    {
                        if (DevMode)
                        {
                            Debug.Log(hit.collider.name +" " +  hitId);
                            SetMarkerAsHidden(hitId, !MarkerIsHidden(hitId));
                        }
                        else 
                            if (MarkerIsHidden(hitId))
                            {
                                ChangeGrabbing(true);
                                grabPoint = hit.point.SetY(0).normalized;
                            }
                            else
                            {
                                Vector3 dir = trans.InverseTransformDirection(hit.transform.position - trans.position);
                                float a = Vector3.SignedAngle(dir.SetY(0).normalized, -Vector3.forward, Vector3.up);
                                angle = Mathf.LerpAngle(angle, angle + a, 1);
                            
                                DrawArea.ShowGame(hit.collider.name);
                                SplitAnim.ShowGame();
                            }
                    }
                }     
        }
            
        if (Input.GetMouseButtonUp(0))
            ChangeGrabbing(false);
                
            
        if (grabbing)
        {
            Vector3 newGrabPoint = grabPoint;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out RaycastHit hit, 10000, mask) && hit.collider == coll)
                newGrabPoint = hit.point.SetY(0).normalized;
            else
                ChangeGrabbing(false);

            Vector3 pos = trans.position;
            float a = Vector3.SignedAngle((grabPoint - pos).normalized, (newGrabPoint - pos).normalized, Vector3.up);
            angle = Mathf.LerpAngle(angle, angle + a, 1);
                
            grabPoint = newGrabPoint;
        }

        if (AndyIdle.IdleMode)
            angle = Mathf.LerpAngle(angle, angle + Time.deltaTime * -1.5f, 1);
    }


    private void ChangeGrabbing(bool doit)
    {
        grabbing = doit;
        
        for (int i = 0; i < markerCount; i++)
            markerColl[i].enabled = !doit;
    }

    
    private void LateUpdate()
    {
        toShader = Mathf.Repeat(angleForce.Update(angle / 360f, Time.deltaTime) + 1000, 110);
        mat.SetFloat(Angle, toShader);
        
        markers.localRotation = Quaternion.AngleAxis(toShader * 360, Vector3.up);

        int best = -1;
        float bestdot = -1;

        Vector3 forward = trans.forward;
        Vector3 right   = trans.right;
        
        Vector3 center = trans.position;
        for (int i = 0; i < markerCount; i++)
        {
            Collider c = markerColl[i];
            Vector3 p = c.transform.position;
            Vector3 dir = p - center;
            dir = (forward * Vector3.Dot(dir, forward) + right * Vector3.Dot(dir, right)).normalized;
            float d = -Vector3.Dot(dir, forward);
            if (d > bestdot && !MarkerIsHidden(i))
            {
                bestdot = d;
                best = i;
            }
            Ray ray = cam.ScreenPointToRay(cam.WorldToScreenPoint(p));
            bool visible = true;
            bool before = c.enabled;
            c.enabled = true;
            if (Physics.Raycast(ray, out RaycastHit hit, 10000, mask))
                if (hit.collider != c)
                    visible = false;
            
            c.enabled = before;
            markerA[i] = ui.UpdateMarker(i, cam.WorldToViewportPoint(p), !DevMode && !MarkerIsHidden(i) && visible || DevMode && visible, d, DevMode && MarkerIsHidden(i));
        }
        
        if(best == -1)
            Debug.Log("OOO");
        
        ui.BestMarker(best);
    }


    public static void ShowBerryPlease()
    {
        Inst.StartCoroutine(Bla("Berry"));
    }
    
    
    public static void Show_Oil_Single_Please()
    {
        Inst.StartCoroutine(Bla("Oil_Single"));
    }


    private static IEnumerator Bla(string gameName)
    {
        Collider c = Inst.markerColl[Inst.map[gameName]];
        Vector3 dir = Inst.trans.InverseTransformDirection(c.transform.position -  Inst.trans.position);
        float a = Vector3.SignedAngle(dir.SetY(0).normalized, -Vector3.forward, Vector3.up);
        Inst.angle = Mathf.LerpAngle(Inst.angle, Inst.angle + a, 1);
        
        yield return new WaitForSeconds(1);
        
        DrawArea.ShowGame(gameName);
        SplitAnim.ShowGame();
    }


    private void OnDisable()
    {
        mat.SetFloat(Angle, 0);
    }


    private int devTouch = 0;
    private float devTouchStart;

    private void DevModeCodeCheck()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 p = cam.ScreenToViewportPoint(Input.mousePosition);
            const float xThresh = .1f;
            float yThresh = xThresh * cam.aspect;
            bool goodTouch = devTouch == 0 && p.x > 1 - xThresh && p.y > 1f - yThresh ||
                             devTouch == 1 && p.x > 1 - xThresh && p.y < yThresh ||
                             devTouch == 2 && p.x < xThresh     && p.y < yThresh ||
                             devTouch == 3 && p.x < xThresh     && p.y > 1f - yThresh;

            if (goodTouch)
            {
                devTouch++;
                if (devTouch == 4)
                {
                    ToggleDevMode();
                    devTouch = 0;
                }
            }
            else
                devTouch = 0;
        }
    }
    
    
    
    private void ToggleDevMode()
    {
        DevMode = !DevMode;
        devText.SetActive(DevMode);
    }


    private PrefInt SavedMarkers = new PrefInt("SavedMarkers");
    private static bool MarkerIsHidden(int id)
    {
        return PlayerPrefs.GetInt("MarkerVis" + id) == 1;
    }

    private void SetMarkerAsHidden(int id, bool hiddden)
    {
        PlayerPrefs.SetInt("MarkerVis" + id, hiddden? 1 : 0);
    }
}
