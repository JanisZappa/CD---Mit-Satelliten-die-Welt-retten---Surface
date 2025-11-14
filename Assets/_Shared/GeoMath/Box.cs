using UnityEngine;


namespace GeoMath
{
    public partial struct Box
    {
        public Vector2 a, b, c, d;

        public Box(Vector2 p, float angle, Vector2 size)
        {
            size *= .5f;
            Vector2 v  = size.Rot(angle);
            Vector2 v2 = size.MultiX(-1).Rot(angle);
            a = p + v;
            b = p + v2;
            c = p - v;
            d = p - v2;
        }
        
        
        public Box Move(Vector2 shift)
        {
            return new Box(a + shift, b + shift, c + shift, d + shift);
        }


        public Vector2 GetUV(Vector2 point)
        {
            Vector2 fromC = point - c;
            Vector2 cd = d - c;
            
            return new Vector2(Vector2.Dot(fromC, (d - c).normalized) / (d - c).magnitude, Vector2.Dot(fromC, (b - c).normalized) / (b - c).magnitude);
        }
    }
}