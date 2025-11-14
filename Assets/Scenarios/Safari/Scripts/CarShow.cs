using UnityEngine;

public class CarShow : MonoBehaviour
{
    private void Start()
    {
        Resolution res = Screen.currentResolution;
        Screen.SetResolution(res.width, res.height, true);
    }
}
