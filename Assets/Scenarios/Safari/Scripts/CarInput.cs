using UnityEngine;


public class CarInput : MonoBehaviour
{
    public Camera cam;
    public Vector2 analogPos;
    public float analogSize;
    public Vector2 dronePos;
    public float droneSize;
    public bool canControll;
    public bool debugDraw;
    
    private float steer, accel;

    private float touchSteer, touchAccel;

    private bool droneButton;
    public bool droneSignal;

    private float Steering =>
        Mathf.Clamp(
            Input.GetAxis("Horizontal") +
            touchSteer
            , -1, 1);

    private float Acceleration =>
        Mathf.Clamp(
            (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W) ? 1 : 0) +
            (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S) ? -1 : 0) +
            touchAccel +
            LR
            , -1, 1);


    private static float LR => Input.GetAxis("LR");

    public Vector2 GetSteerAndAccel(Quaternion carRot)
    {
        if(!canControll || !SafariGame.Running)
            return Vector2.zero;
        
        float dt = .02f / Time.fixedDeltaTime;

        float s = Mathf.Pow(Mathf.Abs(Steering), 1.666666f) * Mathf.Sign(Steering);
        float a = Acceleration;
        steer = Mathf.Lerp(steer, s, Time.fixedDeltaTime * 50 * dt);
        accel = Mathf.Lerp(accel, a, Time.fixedDeltaTime * 50 * dt);
        return new Vector2(steer, accel);
    }
    
    private void LateUpdate()
    {
        //Debug.Log(PopUpManager.ShowingPanel + " " + PopUpManager.ShowingPopUp + " " + canControll);
        
        if (!canControll || !SafariGame.Running)
        {
            touchSteer = 0;
            touchAccel = 0;
            droneButton = false;
            droneSignal = false;
            return;
        }
        
        
        Vector2 cA = cam.ViewportToScreenPoint(analogPos);
        Vector2 cB = cam.ViewportToScreenPoint(new Vector2(1f - analogPos.x, analogPos.y));
        float radius = cam.ViewportToScreenPoint(Vector2.up * analogSize).y;

      
        if(true)
        {
            touchSteer = 0;
            float touchA = Input.GetMouseButton(0) &&
                           AnalogTouchSteer(cA, radius, Input.mousePosition, Vector2.right, ref touchSteer)
                ? 1
                : 0;
            for (int i = 0; i < Input.touchCount; i++)
                if (AnalogTouchSteer(cA, radius, Input.touches[i].position, Vector2.right, ref touchSteer))
                    touchA += 1;

            if (touchA > 0)
                touchSteer /= touchA;

            if (debugDraw)
            {
                DRAW.Circle(cA, radius, 100).ToScreen().SetColor(touchA > 0 ? COLOR.green.spring : COLOR.red.tomato);
                DRAW.Vector(cA + Vector2.up * -radius, Vector2.up * radius * 2).ToScreen()
                    .SetColor(touchA > 0 ? COLOR.green.spring : COLOR.red.tomato);    
            }
        }

        
        if (true)
        {
            touchAccel = 0;
            float touchA = Input.GetMouseButton(0) &&
                           AnalogTouchSteer(cB, radius, Input.mousePosition, Vector2.up, ref touchAccel)
                ? 1
                : 0;
            for (int i = 0; i < Input.touchCount; i++)
                if (AnalogTouchSteer(cB, radius, Input.touches[i].position, Vector2.up, ref touchAccel))
                    touchA += 1;

            if (touchA > 0)
            {
                touchAccel /= touchA;
                touchAccel = touchAccel > 0? 1 : touchAccel < 0 ? -1 : 0;
            }
            else
                touchAccel = 0;
            

            if (debugDraw)
            {
                DRAW.Circle(cB, radius, 100).ToScreen().SetColor(touchA > 0 ? COLOR.green.spring : COLOR.red.tomato);
                DRAW.Vector(cB + Vector2.right * -radius, Vector2.right * radius * 2).ToScreen()
                    .SetColor(touchA > 0 ? COLOR.green.spring : COLOR.red.tomato);    
            }
        }

        
        if (true)
        {
            droneSignal = Input.GetButtonDown("Jump");
            Vector2 cC = cam.ViewportToScreenPoint(dronePos);
            float radius2 = cam.ViewportToScreenPoint(Vector2.up * droneSize).y;

            float touchA = Input.GetMouseButton(0)? DigitalButton(cC, radius2, Input.mousePosition) : 0;
            for (int i = 0; i < Input.touchCount; i++)
                touchA += DigitalButton(cC, radius2, Input.touches[i].position);

            if (touchA > 0)
            {
                if (!droneButton)
                {
                    droneButton = true;
                    droneSignal = true;
                }
            }
            else
                droneButton = false;
            
            if(debugDraw)
                DRAW.Circle(cC, radius2, 100).ToScreen().SetColor(touchA > 0 ? COLOR.green.spring : COLOR.red.tomato);
        }
    }


    private bool AnalogTouchSteer(Vector2 center, float radius, Vector2 pos, Vector2 axis, ref float value)
    {
        Vector2 dir = pos - center;
        if (dir.magnitude > radius * 1.5f)
            return false;
        
        value += Mathf.Clamp(Vector2.Dot(dir, axis) / radius, -1, 1);
        return true;
    }

    
    private float DigitalButton(Vector2 center, float radius, Vector2 pos)
    {
        Vector2 dir = pos - center;
        return dir.magnitude > radius * 1.5f ? 0 : 1;
    }
}
