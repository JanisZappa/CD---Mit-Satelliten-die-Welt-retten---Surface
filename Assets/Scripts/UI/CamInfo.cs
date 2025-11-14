using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamInfo : MonoBehaviour
{
    private static Camera cam;

    public static float Aspect => cam != null ? cam.aspect : 16f / 9f;
    public static float AdjustMulti =>  cam != null ? 1 : 0;//(16f / 9f) / Aspect;
    
    private void Start()
    {
        cam = Camera.main;
    }
}
