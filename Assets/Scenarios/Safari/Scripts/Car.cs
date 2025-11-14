using UnityEngine;


public class Car : MonoBehaviour
{
    public float speed, accel;
    public float steerAngle, steerAccel, steerLessByVelocity;
    
    
    [Space] 
    public Vector2 drag;
    public Vector2 wheels;


    [Space] 
    public Transform camRoot;
    
    public CarInput input;
    public CarTrails trails;
    public CarDust dust;

    [Space] public float steerNoise;
    public float steerNoisePow;
    public float noiseSpeed;

    [Space] public float speedReadout;
    
    private Rigidbody2D rb;
    private Transform trans, visualTrans;

    private float steer, motor;
   
    private CarDetector detector;
    private CarDebug debug;
    
    private readonly Vector3Force CamPos = new Vector3Force().SetSpeed(800).SetDamp(40);
    private readonly Vector3Force CarPos = new Vector3Force().SetSpeed(3400).SetDamp(50);
    private readonly QuaternionForce CarRot = new QuaternionForce().SetSpeed(3400).SetDamp(50);

    private float n, noiseAngle;

    private readonly Transform[] wheelTransforms = new Transform[4];
   
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        trans = transform;
        CamPos.SetValue(trans.position);
        CarPos.SetValue(trans.position);
        
        visualTrans = trans.GetChild(0);
        for (int i = 0; i < 4; i++)
            wheelTransforms[i] = visualTrans.GetChild(i);
        
        visualTrans.parent = null;
        detector = GetComponent<CarDetector>();
        detector.SetDrone(visualTrans.GetChild(4));
        debug = GetComponent<CarDebug>();
    }


    public void ResetCarAndCam(Vector3 spawnPos)
    {
        CamPos.SetValue(spawnPos);
        CarPos.SetValue(spawnPos);
        GetComponent<Rigidbody2D>().Sleep();
        transform.position = spawnPos;
        transform.rotation = Quaternion.identity;
        CarRot.SetValue(Quaternion.identity);
        trails.ResetTrails();
        
        
        steer = 0;
        motor = 0;
    }
    

    private void FixedUpdate()
    {
        Vector3    pos = trans.position;
        Quaternion rot = trans.rotation;
        
        Vector2 inputSteerAccel = input.GetSteerAndAccel(rot);
        
        Vector2 vel = rb.velocity;

        n += Mathf.Pow(vel.magnitude, steerNoisePow) * Time.fixedDeltaTime * noiseSpeed;

        float na = Mathf.PerlinNoise(n, .1f) - Mathf.PerlinNoise(-n, 66556.1f);
        float nb = Mathf.PerlinNoise(n * 2.33f, 53.1f) - Mathf.PerlinNoise(-n * 2.33f, 26556.1f);
        float nc = Mathf.PerlinNoise(n * 4.843f, 333.1f) - Mathf.PerlinNoise(-n * 4.843f, 256.1f);
        
        noiseAngle = Mathf.Lerp(noiseAngle, (na *.5f + nb * .4f + nc * .1f) * steerNoise, Time.fixedDeltaTime * 20);
        SteerUpdate(inputSteerAccel.x);
        MotorUpdate(inputSteerAccel.y);
       
        
        

        Vector3 a = pos + rot * new Vector3(-wheels.x,  -wheels.y);
        Vector3 b = pos + rot * new Vector3( -wheels.x,  wheels.y);
        Vector3 c = pos + rot * new Vector3(  wheels.x,  wheels.y);
        Vector3 d = pos + rot * new Vector3(  wheels.x, -wheels.y);
        
        
        float wheelAngle = (steer + noiseAngle) * (steerAngle - vel.magnitude * steerLessByVelocity);
        Quaternion wheelR = rot * Quaternion.AngleAxis(-wheelAngle, Vector3.forward);

        float dt = (Time.fixedDeltaTime / .02f);
       
    //  Divide by wheels  //
        vel *= .25f * dt;
        
        ApplyWheelDrag(vel, a, rot, 1);
        ApplyWheelDrag(vel, b, wheelR, 0);
        ApplyWheelDrag(vel, c, wheelR, 0);
        ApplyWheelDrag(vel, d, rot, 1);

        vel = rot * Vector3.up * motor * speed;
    //  Divide by wheels  //
        vel *= .25f * dt;
        
        ApplyWheelAccel(vel, a, rot, 1);
        ApplyWheelAccel(vel, b, wheelR, 0);
        ApplyWheelAccel(vel, c, wheelR, 0);
        ApplyWheelAccel(vel, d, rot, 1);
    }


    private void ApplyWheelDrag(Vector3 vel, Vector3 pos, Quaternion rot, float back)
    {
        Vector2 f = rot * Vector3.up;
        Vector2 r = rot * Vector3.right;

        Vector2 d = f * -(Vector2.Dot(vel, f) * drag.x) + r * -(Vector2.Dot(vel, r) * Mathf.Lerp(drag.y, drag.x, back * .45f));
        
        rb.AddForceAtPosition(d, pos, ForceMode2D.Impulse);
    }

    
    private void ApplyWheelAccel(Vector3 vel, Vector3 pos, Quaternion rot, float back)
    {
        Vector2 f = rot * Vector3.up;
        Vector2 r = rot * Vector3.right;
        Vector2 d = f * Vector2.Dot(vel, f) * (1f - drag.x) + r * Vector2.Dot(vel, r) * (1f - Mathf.Lerp(drag.y, drag.x, back * .45f)) ;
        
        rb.AddForceAtPosition(d, pos, ForceMode2D.Impulse);
    }


    private void MotorUpdate(float Acceleration)
    {
        float accStep = Time.fixedDeltaTime * accel;
        float acc = Acceleration * accStep;
        if (Mathf.Approximately(acc, 0))
            acc = Mathf.Min(Mathf.Abs(motor), accStep) * -Mathf.Sign(motor);
        motor = Mathf.Clamp(motor + acc, -.75f, 1);
    }


    private void SteerUpdate(float Steering)
    {
        float accStep = Time.fixedDeltaTime * steerAccel;
        float acc = Steering * accStep;
        if (Mathf.Approximately(acc, 0))
            acc = Mathf.Min(Mathf.Abs(steer), accStep) * -Mathf.Sign(steer);
        steer = Mathf.Clamp(steer + acc, -1, 1);
    }


    private void LateUpdate()
    {
        Vector3    pos = trans.position;
        Quaternion rot = trans.rotation;

        Vector3 carPos = pos;//CarPos.Update(pos, Time.deltaTime);
        const float margin = 13.71094f;
        Vector2 clampedPos = new Vector2(Mathf.Min(Mathf.Abs(carPos.x), 4f * 26 - margin) * Mathf.Sign(carPos.x),
            Mathf.Min(Mathf.Abs(carPos.y), 2.5f * 26 - margin) * Mathf.Sign(carPos.y));
        camRoot.position = clampedPos;//CamPos.Update(clampedPos, Time.deltaTime);
        Quaternion carRot = rot;//CarRot.Update(rot, Time.deltaTime);

        visualTrans.position = carPos;
        visualTrans.rotation = carRot;

        Vector3 a = carPos + carRot * new Vector3(-wheels.x,  -wheels.y);
        Vector3 b = carPos + carRot * new Vector3( -wheels.x,  wheels.y);
        Vector3 c = carPos + carRot * new Vector3(  wheels.x,  wheels.y);
        Vector3 d = carPos + carRot * new Vector3(  wheels.x, -wheels.y);
        
        Vector2 vel = rb.velocity;
        speedReadout = Mathf.Lerp(speedReadout, vel.magnitude, Time.deltaTime * 10);
        
        trails.TrailUpdate(a, b, c, d);
        dust.DustUpdate(carPos, carRot, vel.magnitude);
        detector.DetectorUpdate(carPos, carRot);
        
        
        float wheelAngle = (steer + noiseAngle * 2.5f) * (steerAngle - vel.magnitude * steerLessByVelocity);;
        Quaternion wheelR = carRot * Quaternion.AngleAxis(-wheelAngle, Vector3.forward);

        ApplyWheelsPlacement(0, a, carRot);
        ApplyWheelsPlacement(1, b, wheelR);
        ApplyWheelsPlacement(2, c, wheelR);
        ApplyWheelsPlacement(3, d, carRot);
        
        //debug.DebugUpdate(carPos, carRot, wheelR, a, b, c, d);
    }


    private void ApplyWheelsPlacement(int i, Vector3 pos, Quaternion rot)
    {
        Transform wheel = wheelTransforms[i];
        wheel.position = pos;
        wheel.rotation = rot;
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        motor *= 1 - Mathf.Abs(Vector3.Dot(trans.rotation * Vector3.up, other.contacts[0].normal)) * .5f;
    }
}
