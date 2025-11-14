using UnityEngine;


public class GoodCarInput : MonoBehaviour
{
    public Camera cam;
    public Vector2 analogPos;
    public float analogSize;
    
    private float steer, accel;

    private float Steering =>
        Mathf.Clamp(
            Input.GetAxis("Horizontal")
            , -1, 1);

    private float Acceleration =>
        Mathf.Clamp(
            (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) ? 1 : 0) +
            (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) ? -1 : 0) +
            LR
            , -1, 1);


    private static float LR => Input.GetAxis("LR");

    public Vector2 GetSteerAndAccel()
    {
        //return AnalogAccel(carRot);
        //Debug.Log(LR);
        float dt = .02f / Time.fixedDeltaTime;

        float s = Mathf.Pow(Mathf.Abs(Steering), 2f) * Mathf.Sign(Steering);
        float a = Acceleration;//Mathf.Pow(Mathf.Abs(Acceleration), 1.666666f) * Mathf.Sign(Acceleration);
        steer = Mathf.Lerp(steer, s, Time.fixedDeltaTime * 50 * dt);
        accel = Mathf.Lerp(accel, a, Time.fixedDeltaTime * 50 * dt);
        return new Vector2(steer, accel);
    }
    
}
