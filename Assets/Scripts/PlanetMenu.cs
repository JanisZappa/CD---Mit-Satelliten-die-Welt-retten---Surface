using UnityEngine;


public class PlanetMenu : MonoBehaviour
{
    public MeshRenderer pic;
    private Material mat;
    private static readonly int vis = Shader.PropertyToID("Vis");


    private void Start()
    {
        RenderTexture rT = new RenderTexture(Screen.width, Screen.height, 0);
        GetComponent<Camera>().targetTexture = rT;
        
        mat = pic.material;
        mat.mainTexture = rT;
        
        Camera cam = Camera.main;
        float aspect = cam.aspect;
        float height = cam.orthographicSize * 2;
        float width  = height * aspect;
        
        pic.transform.localScale = new Vector3(width, height);
    }

    
    private void Update()
    {
        mat.SetFloat(vis, SplitAnim.gameVis);
    }
}
