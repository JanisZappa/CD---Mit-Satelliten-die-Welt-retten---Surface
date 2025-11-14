using UnityEngine;


public class CarDust : MonoBehaviour
{
    public float seperation;
    public Vector2 thresh;
    
    private ParticleSystem[] particles;
    private readonly ParticleSystem.EmissionModule[] emission = new ParticleSystem.EmissionModule[2];
    
    private void Start()
    {
        particles = GetComponentsInChildren<ParticleSystem>();

        for (int i = 0; i < 2; i++)
        {
            ParticleSystem p = particles[i];
            emission[i] = p.emission;

        }
    }

    public void DustUpdate(Vector3 carPos, Quaternion carRot, float vel)
    {
        carPos += Vector3.forward * .5f;
        particles[0].transform.position = carPos + carRot * Vector3.up *  seperation;
        particles[1].transform.position = carPos + carRot * Vector3.up * -seperation;

        ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve
        {
            constant = Mathf.Clamp01(Mathf.InverseLerp(thresh.x, thresh.y, vel)) * 20
        };
        for (int i = 0; i < 2; i++)
            emission[i].rateOverTime = curve;
    }
}
