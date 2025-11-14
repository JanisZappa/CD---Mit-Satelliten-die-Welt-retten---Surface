using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ArrangeGroundTiles
{
    [MenuItem("Arrange/Arrange")]
    public static void Arrange()
    {
        if(Selection.activeGameObject == null)
            return;

        Transform t = Selection.activeGameObject.transform;
        int count = t.childCount;
        for (int i = 0; i < count; i++)
        {
            int x = i % 8;
            int y = i / 8;
            
            t.GetChild(i).localPosition = new Vector3(x * 26, y * -26, 0);
        }
    }
}
