using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceCam : MonoBehaviour
{
    private Transform trans;

    // Start is called before the first frame update
    void Start()
    {
        trans = transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        trans.localPosition = Vector3.forward * Mathf.Lerp(-20, -10, Mathf.Pow(SplitAnim.planetZoom, 2));
    }
}
