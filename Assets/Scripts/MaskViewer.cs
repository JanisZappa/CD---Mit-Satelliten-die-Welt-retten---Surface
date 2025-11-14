using UnityEngine;


public class MaskViewer : MonoBehaviour
{
    private bool showMap;
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            showMap = !showMap;
            Set();
        }
    }


    private void OnEnable()
    {
        showMap = false;
        Set();
    }


    private void Set()
    {
        Shader.DisableKeyword("SHOWMAP" + ( showMap? "_OFF" : "_ON"));
        Shader.EnableKeyword( "SHOWMAP" + (!showMap? "_OFF" : "_ON"));
    }
}
