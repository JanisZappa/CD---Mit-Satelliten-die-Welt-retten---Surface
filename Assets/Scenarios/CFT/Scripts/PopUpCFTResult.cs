using UnityEngine;

public class PopUpCFTResult : PopUp
{
    public RectTransform[] rects;
    
    
    protected override void ButtonInit()
    {
        panelButtons[0].onPointerDown.AddListener(() =>
        {
            callback?.Invoke(0);
            ShowPopup();
        });
        
        panelButtons[1].onPointerDown.AddListener(() =>
        {
            callback?.Invoke(1);
            ShowPopup();
        });
    }

    
    public void OnEnable()
    {
        for (int i = 0; i < 3; i++)
            rects[i].localScale = new Vector3(PopUpCFTQuiz.Score[i] * 1f / 5, 1, 1);
    }
}
