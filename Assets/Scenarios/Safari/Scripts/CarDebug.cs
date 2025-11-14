using UnityEngine;

public class CarDebug : MonoBehaviour
{
    public Vector2 wheelSize;
    
    public void DebugUpdate(Vector3 carPos, Quaternion carRot, Quaternion wheelR, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        DRAW.Vector(b, c - b).SetDepth(-1).SetColor(COLOR.red.tomato);
        DRAW.Vector(d, a - d).SetDepth(-1).SetColor(COLOR.red.tomato);
        DRAW.Arrow(carPos + carRot * Vector3.up * -.5f, carRot * Vector3.up, .25f).SetDepth(-1).SetColor(COLOR.purple.orchid);
        
        DrawWheel(a, carRot);
        DrawWheel(d, carRot);
        
        DrawWheel(b, wheelR);
        DrawWheel(c, wheelR);
    }
    
    
    private void DrawWheel(Vector3 pos, Quaternion rot)
    {
        Vector3 a = pos + rot * new Vector3(-wheelSize.x,  -wheelSize.y);
        Vector3 b = pos + rot * new Vector3( -wheelSize.x,  wheelSize.y);
        Vector3 c = pos + rot * new Vector3(  wheelSize.x,  wheelSize.y);
        Vector3 d = pos + rot * new Vector3(  wheelSize.x, -wheelSize.y);
        
        DRAW.Vector(a, b - a).SetDepth(-1).SetColor(COLOR.red.tomato);
        DRAW.Vector(b, c - b).SetDepth(-1).SetColor(COLOR.red.tomato);
        DRAW.Vector(c, d - c).SetDepth(-1).SetColor(COLOR.red.tomato);
        DRAW.Vector(d, a - d).SetDepth(-1).SetColor(COLOR.red.tomato);
    }
}
