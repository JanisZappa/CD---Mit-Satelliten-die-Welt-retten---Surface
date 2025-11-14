
using UnityEngine;
using UnityEngine.UI;


public class CFT_Info : MonoBehaviour
{
    public GameObject gO;
    public ScrollRect scroll;
    public Image content;
    public SuckButton x;

    [Space] 
    public RectTransform arrow;
    public Vector2 arrowOffset;
    
    private Image main;
    
    private RectTransform rect;


    private void Start()
    {
        rect = GetComponent<RectTransform>();
        main = gO.GetComponent<Image>();
        
        x.onPointerDown.AddListener( () =>
        {
            HideInfo();
        });
    }
    
    public void ShowInfo(Vector2 aP, Vector2 o, Sprite a, Sprite b)
    {
        SetPostion(aP, o);
        main.sprite = a;
        content.sprite = b;
        content.SetNativeSize();
        scroll.normalizedPosition = Vector2.one;
        
        gO.SetActive(true);
    }


    public void SetPostion(Vector2 aP, Vector2 o)
    {
        rect.anchoredPosition = aP + o;

        if (Mathf.Abs(o.x) > Mathf.Abs(o.y))
        {
            arrow.anchoredPosition = new Vector2(arrowOffset.x * Mathf.Sign(o.x), -o.y);
            arrow.localRotation = Quaternion.AngleAxis(o.x < 0? 180 : 0, Vector3.forward);
        }
        else
        {
            arrow.anchoredPosition = new Vector2( -o.x, arrowOffset.y * Mathf.Sign(o.y));
            arrow.localRotation = Quaternion.AngleAxis(o.y > 0? 90 : -90, Vector3.forward);
        }
    }


    public void HideInfo()
    {
        CFT_Marker.ActiveMarker = null;
        gO.SetActive(false);
    }
}