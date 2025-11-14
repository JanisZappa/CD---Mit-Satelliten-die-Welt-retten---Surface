using UnityEngine;

public class NeoModelRotate : MonoBehaviour
{
    public Camera screenCam, gameCam;
    
    [Space] 
    public float speed = 1;
    
    private readonly Quaternion rootRot = Quaternion.Euler(11, 0, 5);
    private Quaternion animRot  = Quaternion.Euler(11, 0, 5);
    
    private Transform trans;
    
    private readonly QuaternionForce rotForce = new QuaternionForce(200).SetSpeed(41 * 4).SetDamp(12 * 2 * 2).SetValue(Quaternion.Euler(11, 0, 5));
    
    private bool grabbing, zoomGrabbing;
    private Vector3 grabPoint;
    
    private Collider[] myCollider;
    private int count;
    
    private const float idleThresh = 4;
    private float idleTime;
    private float animA => (Time.unscaledTime - windowTime) * speed + 180;
    private float windowTime;

    private int layermask;
    
    
    private void Start()
    {
        trans = transform;
        
        myCollider = GetComponents<Collider>();
        count = myCollider.Length;
        
        layermask = 1 << gameObject.layer;
    }


    private Quaternion GetRot(Vector3 grab)
    {
        Vector3 dir = (grab - trans.position).normalized;
        return Quaternion.LookRotation(dir, Vector3.up);
    }


    private Plane GetPlane()
    {
        return new Plane(Vector3.forward, trans.position + Vector3.forward * -2);
    }


    private Ray GetRay
    {
        get
        {
            Vector2 p = screenCam.ScreenToViewportPoint(Input.mousePosition);
            p.x = Mathf.Clamp01(.5f + (p.x - .5f) * screenCam.aspect);
            return gameCam.ViewportPointToRay(p);
        }
    }


    private void Update()
    {
        float dt = Time.deltaTime;
        
        if (Input.GetMouseButtonDown(0) && !PopUpManager.ShowingPopUp)
        {
            Ray ray = GetRay;
            //Debug.Log(ray.direction);
            if(Physics.Raycast(ray, out RaycastHit hit, 1000000, layermask))
            {
                for (int i = 0; i < count; i++)
                    if (hit.collider == myCollider[i])
                    {
                        grabbing = true;
                
                        if(GetPlane().Raycast(ray, out float enter))
                            grabPoint = ray.origin + ray.direction * enter;
                        break;
                    }
            }
        }
        
        if (Input.GetMouseButtonUp(0) || PopUpManager.ShowingPopUp)
            grabbing = false;
            
        idleTime = grabbing? 0 : idleTime + Time.deltaTime;
        
        if (grabbing)
        {
            Vector3 newGrabPoint = grabPoint;
            Ray ray = GetRay;
            if(GetPlane().Raycast(ray, out float enter))
                newGrabPoint = ray.origin + ray.direction * enter;
                
            Quaternion a = GetRot(grabPoint);
            Quaternion b = GetRot(newGrabPoint);
            Quaternion c = a * Quaternion.Inverse(b);
            
            animRot   = Quaternion.Inverse(c) * animRot;
            grabPoint = newGrabPoint;
        }
        
        float turnBack = Mathf.Clamp01((idleTime - idleThresh) * .15f) * 1.5f;

        animRot = Quaternion.Slerp(animRot, rootRot * Quaternion.AngleAxis(animA, Vector3.up), dt * turnBack);
        
        trans.rotation = rotForce.Update(animRot, dt);
    }


    public void ShowWindow()
    {
        windowTime = Time.unscaledTime;
        idleTime = 1000;
        grabbing = false;
        animRot = rootRot * Quaternion.AngleAxis(animA, Vector3.up);
        trans = transform;
        trans.rotation = rotForce.SetValue(animRot).SetForce(Quaternion.identity).Value;
    }


    private void OnEnable()
    {
        windowTime = Time.unscaledTime;
        idleTime = 1000;
        grabbing = false;
        animRot = rootRot * Quaternion.AngleAxis(animA, Vector3.up);
        trans = transform;
        trans.rotation = rotForce.SetValue(animRot).SetForce(Quaternion.identity).Value;
    }
}
