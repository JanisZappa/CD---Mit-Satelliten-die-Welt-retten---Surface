using UnityEngine;


public class SetFullscreenRes : MonoBehaviour
{
    public bool aspectWindow;
    
    public Vector2Int res;
    
    private void Start()
    {
        Resolution resolution = Screen.resolutions[Screen.resolutions.Length - 1];
        float aspect = resolution.height * 1f / resolution.width;
        const float compare = 2f / 3f;
        if (Mathf.Abs(aspect - compare) < .01f)
        {
            Screen.SetResolution(resolution.width, resolution.height, true);
            return;
        }
            
        if (aspectWindow)
        {
            float h = resolution.height * .95f;
            float w = resolution.height * .95f * 1.5f;
            Screen.SetResolution(Mathf.RoundToInt(w), Mathf.RoundToInt(h), false);
        }
        else
        {
            if (res.x == 0 || res.y == 0)
            {
                Screen.SetResolution(resolution.width, resolution.height, true);
            }
            else
                Screen.SetResolution(res.x, res.y, true);
        }
        
        
        Destroy(this);
    }
}
