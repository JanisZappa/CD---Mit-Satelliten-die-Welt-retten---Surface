using UnityEngine;
using UnityEngine.UI;


public class BlackFade : MonoBehaviour
{
    private Image image;
    private float alpha;
    
    private void Start()
    {
        image = GetComponent<Image>();
    }

   private void LateUpdate()
    {
        alpha = Mathf.Lerp(alpha, PopUpManager.ShowingPopUp? .6f : 0, Time.deltaTime * 20);
        image.color = new Color(0, 0, 0, alpha);
    }
}
