using UnityEngine;

public class NeoEarthSpin : MonoBehaviour
{
    private static readonly int Angle = Shader.PropertyToID("Angle");
    private Material mat;
    public float speed;

    private float angle;
    
    private void Start()
    {
        MeshRenderer mR =  gameObject.GetComponent<MeshRenderer>();
        mat = Instantiate(mR.material);
        mR.material = mat;
    }

    
    private void Update()
    {
        angle += Time.deltaTime * speed;
        mat.SetFloat(Angle, angle);
    }
}
