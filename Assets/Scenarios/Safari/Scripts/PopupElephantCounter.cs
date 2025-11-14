using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupElephantCounter : PopUp
{
    public Sprite[] elephantSprites;
    private readonly Image[] images = new Image[5];
    
    
    public override void Init()
    {
        base.Init();

        for (int i = 0; i < 5; i++)
        {
            images[i] = transform.GetChild(0).GetChild(i + 1).GetComponent<Image>();
            images[i].sprite = elephantSprites[0];
        }
    }
    
    
    protected override void ButtonInit()
    {
        panelButtons[0].onPointerDown.AddListener( () =>
        {
            callback?.Invoke(0);
            ShowPopup();
        });
        
        if(panelButtons.Length > 1)
            panelButtons[1].onPointerDown.AddListener( () =>
            {
                callback?.Invoke(1);
                ShowPopup();
            });
    }
    
    
    public override void ShowPopup(Action<int> callback = null)
    {
        base.ShowPopup(callback);

        if (callback != null)
            for (int i = 0; i < 5; i++)
                images[i].sprite = elephantSprites[i < SafariUI.Score? 1 : 0];
    }
}
