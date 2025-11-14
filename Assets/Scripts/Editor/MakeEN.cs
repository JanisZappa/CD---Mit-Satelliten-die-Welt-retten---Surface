using UnityEditor;
using UnityEngine;

public class MakeEN : MonoBehaviour
{
    [MenuItem("Language/Append_EN")]
    static void DoSomething()
    {
        Object[] gos = Selection.objects;
        int c = gos.Length;
            
        for (int i = 0; i < c; i++)
        {
            Object go = gos[i];
            if(go.name.Contains("_EN"))
                continue;

            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(go), go.name + "_EN");
        }
        
        AssetDatabase.Refresh();
    }
}
