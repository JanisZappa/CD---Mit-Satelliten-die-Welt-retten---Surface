using UnityEngine;


public class WasserAmount : MonoBehaviour
{
    public void SetAmount(float amount)
    {
        transform.localScale = new Vector3(1, amount, 1);
    }
}
