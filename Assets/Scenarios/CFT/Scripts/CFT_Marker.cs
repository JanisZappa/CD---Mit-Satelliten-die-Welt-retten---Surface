using UnityEngine.UI;
using UnityEngine;

public class CFT_Marker : MonoBehaviour
{
    public CFT_Info info;
    public Sprite[] popup;
    public Sprite[] content;

    [Space] public Vector2 offset;

    private Image image;
    private RectTransform rect;
    private Button button;

    private bool ready;

    public static CFT_Marker ActiveMarker;
    public static bool ShowingInfo => ActiveMarker != null;


    public static void DisableInfo()
    {
        if(ActiveMarker != null)
            ActiveMarker.Hide();
    }

    
    private void Start()
    {
        image = GetComponent<Image>();
        rect  = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        
        button.onClick.AddListener(() =>
        {
            if (ready && ActiveMarker != this)
            {
                ActiveMarker = this;
                int pick = CFTUI.Scenario % 3 + (LanguageSwitch.English? 3 : 0);
                //Debug.Log(pick);
                info.ShowInfo(rect.anchoredPosition, offset, popup[pick], content[pick]);
            }
        });
    }

    
    public void MarkerUpdate(Vector2 aP, Sprite[] ms)
    {
        Vector2 mP = rect.anchoredPosition;

        Vector3 d = aP - mP;
        float dist = Mathf.Max(Mathf.Abs(d.x), Mathf.Abs(d.y));
        ready = dist <= 300;
        image.sprite = ms[ready? 1 : 0];
        image.color = Color.white.A(image.color.a);

        if (ActiveMarker == this && !ready)
            info.HideInfo();
        
        if(Application.isEditor && ActiveMarker == this)
            info.SetPostion(mP, offset);
    }


    public void Hide()
    {
        info.HideInfo();
    }
}
