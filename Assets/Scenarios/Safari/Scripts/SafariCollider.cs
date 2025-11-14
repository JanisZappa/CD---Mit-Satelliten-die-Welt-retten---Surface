using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class SafariCollider : MonoBehaviour
{
    public TextAsset data;
}


#if UNITY_EDITOR
[CustomEditor(typeof(SafariCollider))]
public class SafariColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Do it"))
        {
            SafariCollider sC = target as SafariCollider;
            Transform trans = sC.transform;
            int layer = sC.gameObject.layer;
            
            List<Transform> children = new List<Transform>();
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
                children.Add(trans.GetChild(i));

            for (int i = 0; i < childCount; i++)
                DestroyImmediate(children[i].gameObject);

           
            
            using(MemoryStream m = new MemoryStream(sC.data.bytes))
            using (BinaryReader r = new BinaryReader(m))
            {
                int shapeCount = r.ReadInt32();
                for (int i = 0; i < shapeCount; i++)
                {
                    GameObject go = new GameObject("Path " + i);
                    go.transform.SetParent(trans);
                    go.layer = layer;
                    
                    EdgeCollider2D edge = go.AddComponent<EdgeCollider2D>();

                    int pathLength = r.ReadInt32();
                    Vector2[] path = new Vector2[pathLength];
                    for (int e = 0; e < pathLength; e++)
                        path[e] = new Vector2(r.ReadSingle(), r.ReadSingle());

                    edge.points = path;
                }
            }
            
            EditorUtility.SetDirty(sC.gameObject);
        }
    }
}
#endif
