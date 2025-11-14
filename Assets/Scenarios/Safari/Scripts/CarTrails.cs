using System.Collections.Generic;
using UnityEngine;

public class CarTrails : MonoBehaviour
{
    private class Trail
    {
        private int i;
        private const float thresh = .15f, threshMulti = 1f / thresh;
        private const int count = 512;
        private readonly Vector2[] pts = new Vector2[count];

        public void Reset(Mesh mesh)
        {
            i = 0;
            if(mesh != null)
                mesh.Clear();
        }

        public void Update(Vector2 pos)
        {
            if (i == 0)
            {
                pts[i++] = pos;
                return;
            }

            Vector2 last = pts[(i - 1) % count];
            Vector2 dir = pos - last;
            int steps = Mathf.FloorToInt(dir.magnitude * threshMulti);
            if(steps == 0)
                return;

            dir = dir.normalized;
            for (int e = 0; e < steps; e++)
                pts[i++ % count] = last + dir * thresh * (e + 1);
        }
        
        
        private static readonly List<Vector3> verts = new List<Vector3>();
        private static readonly List<Vector2> uvs = new List<Vector2>();
        private static readonly List<int> tris = new List<int>();

        public void UpdateMesh(Mesh mesh)
        {
            if(i < 2)
                return;
            
            mesh.Clear();
            verts.Clear();
            uvs.Clear();
            tris.Clear();
            
            Vector2 last = pts[(i - 1) % count];
            Vector2 next = pts[ i % count];
            AddMeshStep(last, Quaternion.LookRotation(next - last, Vector3.forward), 0);
            
            
            int max = Mathf.Min(i, count);
            for (int e = 1; e < max; e++)
            {
                Vector2 pt = pts[(i - 1 - e) % count];
                AddMeshStep(pt, Quaternion.LookRotation(pt - last, Vector3.forward), e * 1f / count);
                last = pt;

                int a = (e - 1) * 2;
                int b = e * 2;
                int c = e * 2 + 1;
                int d = (e - 1) * 2 + 1;
                
                tris.Add(a);
                tris.Add(b);
                tris.Add(c);
                
                tris.Add(c);
                tris.Add(d);
                tris.Add(a);
            }
            
            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
        }

        private static void AddMeshStep(Vector3 p, Quaternion r, float l)
        {
            const float halfWidth = .08f;
            Vector3 dir = r * Vector3.right * halfWidth;
            verts.Add(p + dir);
            verts.Add(p - dir);
            uvs.Add(new Vector2(0, l));
            uvs.Add(new Vector2(1, l));
        }
    }
    
    
    private readonly Trail[] trails = CollectionInit.Array<Trail>(4);

    public void TrailUpdate(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        //return;
        trails[0].Update(a);
        trails[1].Update(b);
        trails[2].Update(c);
        trails[3].Update(d);

        
        for (int i = 0; i < 4; i++)
            trails[i].UpdateMesh(meshes[i]);
    }


    public void ResetTrails()
    {
        for (int i = 0; i < 4; i++)
            trails[i].Reset(meshes[i]);
    }


    private readonly Mesh[] meshes = new Mesh[4];

    private void Start()
    {
        MeshFilter[] mF = GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < 4; i++)
        {
            Mesh mesh = new Mesh {name = "Trail" + i};
            mesh.MarkDynamic();
            mF[i].mesh = mesh;
            meshes[i] = mesh;
        }
    }
}
