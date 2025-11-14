using UnityEngine;


public class GrabWindow : MonoBehaviour
{
    public static void ResetMat(int id)
    {
        int matID = Shader.PropertyToID(id == 0? "windowA" : "windowB");
        
        Shader.SetGlobalVector(matID, Vector2.one * 100000);
    }
    
    private Vector2 pos;
    private Transform trans;
    
    private bool beingDragged;
    private Vector2 offset;
    
    private Vector2 center, area;
    
    public Vector2 windowSizeA, windowSizeB;
    [Space]
    public float yOffset;
    
    private readonly Vector2Force force = new Vector2Force(200).SetSpeed(900).SetDamp(40);
    
    
    public GrabWindow Init(Vector2 pos, Vector2 area)
    {
        this.pos = pos;
        
        center = pos;
        this.area = area;
        
        trans = transform;
        
        trans.position = pos;
        force.SetValue(pos);
        
        return this;
    }


    public void ResetPos(Vector3 pos)
    {
        this.pos = pos;
        trans.position = pos;
        force.SetValue(pos);
    }

    
    private bool InsideRect(Vector2 center, Vector2 size, Vector2 check)
    {
        Vector2 min = center - size;
        Vector2 max = center + size;
        
        return check.x > min.x && check.x < max.x && check.y > min.y && check.y < max.y;
    }
    
    
    private static void Draw(Vector2 center, Vector2 size, Color color)
    {
        Vector2 min = center - size;
        Vector2 max = center + size;

        DRAW.Box(min, max).SetColor(color).SetDepth(-1);
    }
    
    
    public bool StartGrabbing(Vector2 finger)
    {
        if(beingDragged || PopUpManager.ShowingPanel)
            return false;
        
        offset =  pos - finger;
       
        beingDragged = InsideRect(pos, windowSizeA, finger) && 
                      !InsideRect(pos + new Vector2(0, yOffset), windowSizeB, finger);
        
        return beingDragged;
    }


    public void GrabUpdate(Vector2 finger)
    {
        pos = finger + offset;
        
        Vector2 centerOffset = pos - center;
        Vector2 shiftPos = center + new Vector2(Mathf.Min(area.x * .5f -  windowSizeA.x, Mathf.Abs(centerOffset.x)) * Mathf.Sign(centerOffset.x), 
                                                Mathf.Min(area.y - windowSizeA.y, Mathf.Abs(centerOffset.y)) * Mathf.Sign(centerOffset.y));
        pos = shiftPos;
        offset = pos - finger;

        return;
        Draw(pos, windowSizeA, Color.red);
        Draw(pos + new Vector2(0, yOffset), windowSizeB, Color.green);
    }


    public void WindowUpdate(bool show)
    {
        Vector2 p = force.Update(pos, Time.deltaTime);
        trans.position = p;
        visPos = show? p : Vector2.one * 100000;
    }
    
    private Vector2 visPos;
    public Vector2 VisPos => visPos;


    public void StopGrabbing()
    {
        beingDragged = false;
    }
}
