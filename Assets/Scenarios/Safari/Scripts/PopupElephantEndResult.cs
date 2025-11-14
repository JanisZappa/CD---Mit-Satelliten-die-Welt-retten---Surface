using System;
using UnityEngine;
using UnityEngine.UI;

public class PopupElephantEndResult : PopUp
{
    public Sprite[] elephantSprites;
    public Sprite[] toolSprites;
    private readonly Image[] images = new Image[7];
    public Sprite[] panelSprites;
    private Image panel;
    
    
    public override void Init()
    {
        base.Init();

        for (int i = 0; i < 7; i++)
        {
            images[i] = transform.GetChild(0).GetChild(i + 1).GetComponent<Image>();
            images[i].sprite = elephantSprites[0];
        }

        panel = transform.GetChild(0).GetChild(0).GetComponent<Image>();
    }
    
    
    protected override void ButtonInit()
    {
        panelButtons[0].onPointerDown.AddListener( () =>
        {
            callback?.Invoke(0);
            ShowPopup();
        });
        
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
        {
            for (int i = 0; i < 5; i++)
                images[i].sprite = elephantSprites[i < SafariUI.Score? 1 : 0];

            images[5].sprite = toolSprites[SafariUI.SelectedTools[0] ? 0 : 1];
            images[6].sprite = toolSprites[SafariUI.SelectedTools[2] ? 2 : SafariUI.SelectedTools[3]? 3 : 4];

            panel.sprite = panelSprites[(SafariUI.Score == 5 ? 1 : 0) + (LanguageSwitch.English? 2 : 0)];
        }   
    }
}
