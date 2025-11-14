using System.IO;
using UnityEngine;

public class FloorMaster : MonoBehaviour
{
    public TextAsset data;
    public Material mat;

    private const int steps = 64, tSteps = steps - 1;
    private const int vCount = steps * steps;
    private Vector3[] verts   = new Vector3[vCount];
    private Vector3[] normals = new Vector3[vCount];
    private Color[] colors    = new Color[vCount];
    private int[] tris = new int[tSteps * tSteps * 6];
    private Vector2[] uvs   = new Vector2[vCount];
    
    
    private void Start()
    {
        int index = 0;
        for (int z = 0; z < tSteps; z++)
        for (int x = 0; x < tSteps; x++)
        {
            tris[index++] = z * steps + x;
            tris[index++] = (z + 1) * steps + x;
            tris[index++] = (z + 1) * steps + x + 1;
            
            tris[index++] = (z + 1) * steps + x + 1;
            tris[index++] = z * steps + x + 1;
            tris[index++] = z * steps + x;
        }
        // Debug.Log(index);
        
        
        Transform parent = transform;
        
        using(MemoryStream m = new MemoryStream(data.bytes))
        using(BinaryReader r = new BinaryReader(m))
        {
            int count = r.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                Vector3 tilePos = new Vector3(r.ReadSingle(), r.ReadSingle(), -r.ReadSingle());
                
                GameObject tile = new GameObject("Tile");
                tile.transform.position = tilePos;
                tile.transform.parent = parent;

                const float vStep = 1f / (steps - 1) * 128;
                for (int e = 0; e < vCount; e++)
                {
                    int x = e % steps;
                    int z = e / steps;
                    verts[e] = new Vector3(x * vStep, r.ReadSingle(), z * vStep);
                }
                
                for (int e = 0; e < vCount; e++)
                {
                    int cValue = r.ReadInt32();
                    int bValue = cValue / (256 * 256) % 256;
                    int gValue = cValue / 256 % 256;
                    int rValue = cValue % 256;
                    colors[e] = new Color32((byte) rValue, (byte) gValue, (byte) bValue, 0);
                }

                for (int e = 0; e < vCount; e++)
                    normals[e] = (Hou.Rot(r.ReadInt32()) * Vector3.up);//.MultiX(-1).MultiZ(-1);
                
                for (int e = 0; e < vCount; e++)
                    uvs[e] = new Vector2(r.ReadSingle(), 0);
                
                Mesh mesh = new Mesh();
                mesh.SetVertices(verts);
                mesh.SetTriangles(tris, 0);
                mesh.SetNormals(normals);
                mesh.SetColors(colors);
                mesh.SetUVs(0, uvs);
                mesh.RecalculateBounds();

                tile.AddComponent<MeshFilter>().mesh = mesh;
                tile.AddComponent<MeshRenderer>().material = mat;
                tile.AddComponent<MeshCollider>().sharedMesh = mesh;
                //tile.isStatic = true;
            }
        }
    }
}
