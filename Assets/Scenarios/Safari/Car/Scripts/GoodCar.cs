using UnityEngine;
using UnityEngine.SceneManagement;


public class GoodCar : MonoBehaviour
{
    public float speed, accel, stickLessByVelocity;
    public float steerAngle, steerAccel;

    [Space] 
    [Range(0, 1)]
    public float frontBack;
    
    [Space]
    public Vector2 wheelLayout;
    public WheelSettings wheel;
    public SpringSettings spring;
    public Mesh rayMesh;
    public GameObject wheelPrefab;

    [Space] 
    public GoodCarInput input;

    private Transform trans;
    private Rigidbody rb;

    private readonly Vector3[] wheelPositions = new Vector3[4];
    private readonly Vector3[] wheelDrawPts = new Vector3[30];

    private Vector3[] wheelRays;
    private int rayCount;

    private readonly Transform[] wheelTrans = new Transform[4];
    
    private prefFloat CarX = new prefFloat("CarX");
    private prefFloat CarY = new prefFloat("CarY");
    private prefFloat CarZ = new prefFloat("CarZ");

    private Vector3 SavedCarPos
    {
        get => new Vector3(CarX, CarY, CarZ);
        set
        {
            CarX.Set(value.x);
            CarY.Set(value.y);
            CarZ.Set(value.z);
        }
    }
    
    
    public float steer, motor;
    
    
    private void Start()
    {
        Physics.gravity = new Vector3(0, -15f, 0);
        
        trans = transform;

        Vector3 spawnPos = SavedCarPos;
        if ((spawnPos - Vector3.zero).sqrMagnitude > .01f)
            trans.position = spawnPos;
        
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.up * .175f + Vector3.forward * -.1f;
        
        int i = 0;
        for (int z = 0; z < 2; z++)
        for (int x = 0; x < 2; x++)
            wheelPositions[i++] = new Vector3(wheelLayout.x * (-1 + x * 2), spring.length * .75f, wheelLayout.y * (-1 + z * 2));

        wheelRays = rayMesh.vertices;
        rayCount  = wheelRays.Length;
        for (int e = 0; e < rayCount; e++)
        {
            Vector3 r = wheelRays[e];
            wheelRays[e] = new Vector3(r.x * wheel.thickness, r.y * wheel.radius, r.z * wheel.radius);
        }

        for (int e = 0; e < 4; e++)
            wheelTrans[e] = Instantiate(wheelPrefab).transform;
    }

    
    private void LateUpdate()
    {
        Vector3 pos = trans.position;
        Quaternion rot = trans.rotation;
        Vector3 down = rot * Vector3.down;
        
        float noiseAngle = 0;
        float wheelAngle = (steer + noiseAngle) * steerAngle;
        Quaternion wheelR = rot * Quaternion.AngleAxis(wheelAngle, Vector3.up);

        //for (int i = 0; i < 4; i++)
        //    DrawWheel(pos + rot * wheelPositions[i], i < 2? rot : wheelR, down);

        int checkValue = 0;
        for (int i = 0; i < 4; i++)
            checkValue += PlaceWheel(wheelTrans[i], pos + rot * wheelPositions[i], (i < 2? rot : wheelR) * Quaternion.AngleAxis(i % 2 == 0? 180 : 0, Vector3.up), down);

        if (checkValue == 4)
            SavedCarPos = pos;
            
        //DRAW.Box(pos + rot * boxPos, boxSize, rot).SetColor(COLOR.turquois.bright);

        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    private int PlaceWheel(Transform wT, Vector3 pos, Quaternion rot, Vector3 down)
    {
        Vector3 end;
        WheelCheck check = WheelRayCastHit(pos, rot, down);
        if (check.gounded)
            end = pos + down * (spring.length - check.springOffset);
        else
            end = pos + down * spring.length;

        wT.position = end;
        wT.rotation = rot;

        return check.gounded? 1 : 0;
    }


    private void DrawWheel(Vector3 pos, Quaternion rot, Vector3 down)
    {
        Vector3 end;
        WheelCheck check = WheelRayCastHit(pos, rot, down);
        if (check.gounded)
            end = pos + down * (spring.length - check.springOffset);
        else
            end = pos + down * spring.length;


        Color c = COLOR.red.tomato;
        DRAW.Vector(pos, end - pos).SetColor(c);

        DrawWheelSide(end, rot, 1);
        DrawWheelSide(end, rot, -1);
    }


    private void DrawWheelSide(Vector3 end, Quaternion rot, float side)
    {
        Color c = COLOR.red.tomato;
        Vector3 u = new Vector3(wheel.thickness * side, wheel.radius, 0);

        for (int i = 0; i < 30; i++)
            wheelDrawPts[i] = end + rot * Quaternion.AngleAxis(12 * i, Vector3.right) * u;

        Vector3 a = wheelDrawPts[29];
        for (int i = 0; i < 30; i++)
        {
            Vector3 b = wheelDrawPts[i];
            DRAW.Vector(a, b - a).SetColor(c);

            a = b;
        }
    }
    

    private void FixedUpdate()
    {
        Vector2 inputSteerAccel = input != null? input.GetSteerAndAccel() : Vector2.zero;
        
        Vector3    pos  = trans.position;
        Quaternion rot  = trans.rotation;
        Vector3    down = rot * Vector3.down;

        float vel = Mathf.Abs(Vector3.Dot(rb.velocity, rot * Vector3.forward));
        
        SteerUpdate(inputSteerAccel.x, vel);
        MotorUpdate(inputSteerAccel.y);
       

        float noiseAngle = 0;
        float wheelAngle = (steer + noiseAngle) * steerAngle;
        Quaternion wheelR = rot * Quaternion.AngleAxis(wheelAngle, Vector3.up);
        
        
        for (int i = 0; i < 4; i++)
        {
            Vector3    p = pos + rot * wheelPositions[i];
            Quaternion r = i < 2 ? rot : wheelR;
            WheelCheck check = WheelRayCastHit(p, r, down);
            if(!check.gounded)
                continue;

            Vector3  v = rb.GetPointVelocity(p);
            Vector3 add = WheelDamp(rot * Vector3.up, v, check.springOffset) +
                          ApplyWheelAccel(r * Vector3.right, check.groundNormal, v,
                              Mathf.Lerp(1f - frontBack, frontBack, i >= 2 ? 1 : 0f));
            
            rb.AddForceAtPosition(add + ApplyWheelDrag(r * Vector3.right, check.groundNormal, v), p);
        }
        
        //rb.
        
    }
    
    
    
    
    private void MotorUpdate(float Acceleration)
    {
        float accStep = Time.fixedDeltaTime * accel;
        float acc = Acceleration * accStep;
        if (Mathf.Approximately(acc, 0))
            acc = Mathf.Min(Mathf.Abs(motor), accStep) * -Mathf.Sign(motor);
        motor = Mathf.Clamp(motor + acc, -.75f, 1);
    }


    private void SteerUpdate(float Steering, float vel)
    {
        float slipMulti = 1f - Mathf.Clamp01(vel * stickLessByVelocity * .02f);
        float accStep = Time.fixedDeltaTime * steerAccel;
        float acc = Steering * accStep;
        if (Mathf.Approximately(acc, 0))
            acc = Mathf.Min(Mathf.Abs(steer), accStep) * -Mathf.Sign(steer);
        steer = Mathf.Clamp(steer + acc, -slipMulti, slipMulti);
    }
    
    
    private Vector3 ApplyWheelDrag(Vector3 right, Vector3 normal, Vector3 vel)
    {
        //vel *= 1.5f;
        //normal = Vector3.up;
        Vector3 f = Vector3.Cross(right, normal).normalized;
        Vector3 r = Vector3.Cross(normal, f).normalized;

        float rDot = Vector3.Dot(vel, r);
        float slipMulti = 1f - Mathf.Clamp01(Mathf.Abs(rDot) * stickLessByVelocity * .6f);
        //slipMulti = 1;
        return f * -(Vector3.Dot(vel, f) * wheel.drag.forward) +
               r * -(rDot * Mathf.Lerp(wheel.drag.forward, wheel.drag.sideways, slipMulti))// * 3;
          + vel * -.025f;
        //+ right * -Vector3.Dot(vel, right) * wheel.drag.sideways;
    }

    
    private Vector3 ApplyWheelAccel(Vector3 right, Vector3 normal, Vector3 vel, float factor)
    {
        //normal = Vector3.up;
        Vector3 f = Vector3.Cross(right, normal).normalized;
        Vector3 r = Vector3.Cross(normal, f).normalized;

        float rDot = Vector3.Dot(vel, r);
        float slipMulti = 1f - Mathf.Clamp01(Mathf.Abs(rDot) * stickLessByVelocity * .05f);
        //slipMulti = 1;
        return f * motor * speed * factor * slipMulti;
    }


    private Vector3 WheelDamp(Vector3 up, Vector3 pointVel, float springOffset)
    {
        float vel = Vector3.Dot(up, pointVel);

        float force = springOffset * spring.strength - vel * spring.damping;
        
        return up * force;
    }
    
    private readonly RaycastHit[] hits = new RaycastHit[100];


    private WheelCheck WheelRayCastHit(Vector3 pos, Quaternion rot, Vector3 down)
    {
        float hitDist = float.MaxValue;
        bool foundHit = false;
        Vector3 normal = Vector3.up;
        
        for (int r = 0; r < rayCount; r++)
        {
            Vector3 wheelRay = wheelRays[r];
            float d = -wheelRay.y;
            wheelRay.y = 0;
            int hitCount = Physics.RaycastNonAlloc(new Ray(pos + rot * wheelRay, down), hits, spring.length + d);
            
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.rigidbody != rb)
                {
                    float dist = hit.distance - d;
                    if (hitDist > dist)
                    {
                        hitDist = dist;
                        normal  = hit.normal;
                    }
               
                    foundHit = true;
                }
            }
        }
        
        return new WheelCheck(foundHit, foundHit ? spring.length - hitDist : 0, normal);
    }
    
    
    
    
    
    private WheelCheck SpringRaycastHit(Vector3 pos, Quaternion rot, Vector3 down)
    {
        int hitCount = Physics.RaycastNonAlloc(new Ray(pos, down), hits, spring.length + wheel.radius);
        float hitDist = float.MaxValue;
        bool foundHit = false;
        Vector3 normal = Vector3.up;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.rigidbody != rb)
            {
                if (hitDist > hit.distance)
                {
                    hitDist = hit.distance;
                    normal  = hit.normal;
                }
               
                foundHit = true;
            }
        }

        return new WheelCheck(foundHit, foundHit ? spring.length + wheel.radius - hitDist : 0, normal);
    }
    
    
    private void OnCollisionEnter(Collision other)
    {
        motor *= 1 - Mathf.Abs(Vector3.Dot(trans.rotation * Vector3.forward, other.contacts[0].normal)) * .5f;
    }


    private struct WheelCheck
    {
        public bool gounded;
        public float springOffset;
        public Vector3 groundNormal;

        public WheelCheck(bool gounded, float springOffset, Vector3 groundNormal)
        {
            this.gounded = gounded;
            this.springOffset = springOffset;
            this.groundNormal = groundNormal;
        }
    }
    
}
