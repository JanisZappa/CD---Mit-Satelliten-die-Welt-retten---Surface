using UnityEngine;

[ExecuteInEditMode]
public class AtmosphereSetter : MonoBehaviour
{
    public Transform earth;
    public Transform sun;
    
    private static readonly int Earth = Shader.PropertyToID("Earth");
    private static readonly int Sun   = Shader.PropertyToID("Sun");
    private static readonly int EarthUp = Shader.PropertyToID("EarthUp");
    private static readonly int EarthForward = Shader.PropertyToID("EarthForward");
    private static readonly int EarthRight = Shader.PropertyToID("EarthRight");


    private void Update()
    {
        Shader.SetGlobalVector(Earth, earth.position);
        Shader.SetGlobalVector(Sun, sun.position);
        
        Shader.SetGlobalVector(EarthUp,      earth.up);
        Shader.SetGlobalVector(EarthForward, earth.forward);
        Shader.SetGlobalVector(EarthRight,   earth.right);
    }
}
