using UnityEngine;


public class GoodSun : MonoBehaviour
{
    public float speed;

    private readonly prefFloat t = new prefFloat("sunTime");
    

    private void Update()
    {
        t.Set((t + Time.deltaTime * speed * (Input.GetKey(KeyCode.Alpha0)? 100 : 1))%1);
        
        transform.localRotation = Quaternion.AngleAxis(t * 180, Vector3.up);
    }
}
