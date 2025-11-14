
using UnityEngine;

public class GoodCarCam : MonoBehaviour
{
    public Vector2 fov;
    public float fovMulti;
    public float distMulti;
    
    public float planeHeight;
    public float lookAhead;
    public Transform target;
    private Rigidbody rB;

    private Transform trans, camTrans;
    private Camera cam;
    private prefBool zoom = new prefBool("GoodCarCamZoom");
    
    
    private Vector3Force force = new Vector3Force(200).SetSpeed(36).SetDamp(6);
    private Vector3Force force2 = new Vector3Force(200).SetSpeed(3).SetDamp(6);
    private FloatForce force3 = new FloatForce(200).SetSpeed(36).SetDamp(6);

    private QuaternionForce camForce = new QuaternionForce(200).SetSpeed(35).SetDamp(200);
    private Quaternion camRot;
    private static readonly int Focus = Shader.PropertyToID("Focus");


    private void Start()
    {
        trans = transform;
        camTrans = trans.GetChild(0);
        cam = camTrans.GetComponent<Camera>();
        rB = target.GetComponent<Rigidbody>();

        camRot = camTrans.rotation;
    }
    
    
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) || Input.GetButtonDown("Fire1"))
            zoom.Toggle();

        Vector3 vel = rB.velocity.SetY(0);
        float fV = Mathf.Lerp(fov.x, fov.y, zoom ? 1 : 0) *
                   (1f + force3.Update(vel.magnitude, Time.deltaTime) * fovMulti);
                            
        float aspect = cam.aspect;
        
        
        trans.rotation = Quaternion.identity;
        Quaternion rot = Quaternion.identity;//Quaternion.LookRotation(target.forward.MultiY(0).normalized, Vector3.up);
        camTrans.rotation = camForce.Update(rot, Time.deltaTime) * camRot;
        Vector3 f = camTrans.forward;
        Vector3 targetPos = target.position;
        Shader.SetGlobalVector(Focus, targetPos);
        
        Vector3 targetPlane = targetPos + f * -Mathf.Abs((planeHeight - targetPos.y) / f.y);
        Vector3 camPlane = camTrans.localPosition + f * Mathf.Abs((planeHeight - camTrans.localPosition.y) / f.y);
        trans.position = force.Update(targetPlane - camPlane, Time.deltaTime) +
                         force2.Update(vel * lookAhead, Time.deltaTime);
                         
        cam.fieldOfView = 
            (Camera.HorizontalToVerticalFieldOfView(Camera.VerticalToHorizontalFieldOfView(fV, aspect) * (16f / 9f / aspect), aspect) * .5f + fV * .5f) *
            (1f + (targetPos - camTrans.position).magnitude * distMulti);
    }
}
