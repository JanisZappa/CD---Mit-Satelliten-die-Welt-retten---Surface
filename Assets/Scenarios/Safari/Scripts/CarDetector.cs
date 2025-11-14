using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class CarDetector : MonoBehaviour
{
    public GameObject ringPrefab;
    public Vector2 radii;
    public float droneOffset;
        
    private Transform ring;
    private static readonly int Radius = Shader.PropertyToID("_Radius");
    private static readonly int CarPos = Shader.PropertyToID("_CarPos");
    public bool droning, charging, flying;

    [Space] 
    public Vector3 currentPos;
    public float currentRadius;

    [Space] public CarInput input;

    private Quaternion currentRot;
    
    private float heightT;
    private Transform drone;
    private Quaternion droneRot;
    
    private Vector3 droneStartDir = Vector3.down;
    
    private readonly Vector3Force centerForce = new Vector3Force(300).SetSpeed(150).SetDamp(5);

    
    public void SetDrone(Transform drone)
    {
        drone.parent = null;
        droneRot = Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.forward);
        this.drone = drone;
    }

    
    private void Start()
    {
        if(!Application.isPlaying)
            return;
        
        GameObject gO = Instantiate(ringPrefab);
        ring = gO.transform;
        currentRadius = radii.x;
    }

    
    public void DetectorUpdate(Vector3 carPos, Quaternion carRot)
    {
        ring.position = carPos;
        Vector3 c = centerForce.Update(carPos, Time.deltaTime);

        currentPos = carPos;
        currentRot = carRot;
        
        
        if (!droning && input.droneSignal)
            StartCoroutine(FullAnim());

        
        Vector3 backPos = currentPos + currentRot * Vector3.up * droneOffset;

        float l = Mathf.SmoothStep(0, 1, Mathf.PingPong(heightT * 2, 1));
        float rise = 1f - Mathf.Pow(1f - l, 4);
        
        Vector3 dir = Quaternion.AngleAxis(Mathf.SmoothStep(0, 1, heightT) * duration * 50, Vector3.forward) * droneStartDir * (1 - Mathf.Pow(1 - l, 3)) * 9 * rise;

        Vector3 root = Vector3.Lerp(backPos, currentPos, l);
        Vector3 dronePos = root + dir;

        
        drone.position = Vector3.Lerp(backPos, Vector3.Lerp(currentPos, centerForce.Update(currentPos, Time.deltaTime), 1f - Mathf.Pow(1f - l, 2)), l) + dir + Vector3.forward * -(1 + rise * 50);

        if (flying)
            drone.rotation = droneRot;
        else
            drone.rotation = carRot * (droneRot * Quaternion.AngleAxis(charging? 180 : 0, Vector3.up));
        
        drone.localScale = Vector3.one * (1 + rise * .5f);
        
        currentRadius = radii.x + (dronePos - root).magnitude * .4875f;
        
        Shader.SetGlobalFloat(Radius, currentRadius);
        Shader.SetGlobalVector(CarPos, carPos);
    }

    private const float duration = 20f;
    private const float tSpeed = 1f / duration;
    

    private IEnumerator FullAnim()
    {
        int myID = SafariGame.GameID;
        droning = true;
        flying  = true;
        
        float t = 0;
        Quaternion oldRot = currentRot * droneRot;
        droneRot = oldRot;

        droneStartDir = currentRot * Vector3.down;
        
        while (t < 1)
        {
            if (myID != SafariGame.GameID)
            {
                ResetValues();
                yield break;
            }
            
            if (SafariGame.Running)
            {
                t += Time.deltaTime * tSpeed;

                heightT = t;
                droneRot = oldRot * Quaternion.AngleAxis(Mathf.SmoothStep(0, duration * 65, t), Vector3.forward);
            }
            
            yield return null;
        }

        droneRot = Quaternion.Inverse(currentRot) * droneRot;
        
        flying = false;

        heightT = 0;

        t = 0;

        charging = true;

        while (t < 10)
        {
            if (myID != SafariGame.GameID)
            {
                ResetValues();
                yield break;
            }
            
            if (SafariGame.Running)
                t += Time.deltaTime;
            
            yield return null;
        }

        charging = false;
        droning  = false;
    }


    public float GetCharge => charging ? 0 : droning ? 1f - heightT : 1;


    private void OnDisable()
    {
        Shader.SetGlobalFloat(Radius, 1000000);
        Shader.SetGlobalVector(CarPos, Vector4.zero);
        StopCoroutine(FullAnim());
        ResetValues();
    }


    private void ResetValues()
    {
        droning  = false;
        flying   = false;
        charging = false;
        heightT = 0;
    }
}
