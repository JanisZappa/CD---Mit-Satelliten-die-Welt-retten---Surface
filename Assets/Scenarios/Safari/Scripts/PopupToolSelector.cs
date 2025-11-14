using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;


public class PopupToolSelector : PopUp
{
    public Sprite[] toolSprites;
    private readonly Image[] images = new Image[5];

    private Image backImage;
    
    
    public override void Init()
    {
        base.Init();

        SetButtonGraphics();
    }


    private void SetButtonGraphics()
    {
        for (int i = 0; i < 5; i++)
        {
            images[i] = transform.GetChild(0).GetChild(i + 1).GetComponent<Image>();
            images[i].sprite = toolSprites[i + (SafariUI.SelectedTools[i]? 5 : 0)];
        }
    }

    
    protected override void ButtonInit()
    {
        panelButtons[0].onPointerDown.AddListener( () =>
        {
            if (!SafariUI.SelectedTools[0])
            {
                SafariUI.SelectedTools[0] = true;
                SafariUI.SelectedTools[1] = false;
                SetButtonGraphics();
            }
        });
        panelButtons[1].onPointerDown.AddListener( () =>
        {
            if (!SafariUI.SelectedTools[1])
            {
                SafariUI.SelectedTools[0] = false;
                SafariUI.SelectedTools[1] = true;
                SetButtonGraphics();
            }
        });
        
        panelButtons[2].onPointerDown.AddListener( () =>
        {
            if (!SafariUI.SelectedTools[2])
            {
                SafariUI.SelectedTools[2] = true;
                SafariUI.SelectedTools[3] = false;
                SafariUI.SelectedTools[4] = false;
                SetButtonGraphics();
            }
        });
        panelButtons[3].onPointerDown.AddListener( () =>
        {
            if (!SafariUI.SelectedTools[3])
            {
                SafariUI.SelectedTools[2] = false;
                SafariUI.SelectedTools[3] = true;
                SafariUI.SelectedTools[4] = false;
                SetButtonGraphics();
            }
        });
        panelButtons[4].onPointerDown.AddListener( () =>
        {
            if (!SafariUI.SelectedTools[4])
            {
                SafariUI.SelectedTools[2] = false;
                SafariUI.SelectedTools[3] = false;
                SafariUI.SelectedTools[4] = true;
                SetButtonGraphics();
            }
        });
        
        panelButtons[5].onPointerDown.AddListener( () =>
        {
            callback?.Invoke(0);
            ShowPopup();
        });

        backImage = panelButtons[6].GetComponent<Image>();
        backImage.enabled = false;

    }
    
    
    public override void ShowPopup(Action<int> callback = null)
    {
        base.ShowPopup(callback);

        if (callback != null)
        {
            SetButtonGraphics();

            if (!SafariUI.FirstTimeTools)
            {
                panelButtons[6].onPointerDown.AddListener(() =>
                {
                    callback?.Invoke(1);
                    ShowPopup();
                });
            }

            backImage.enabled = !SafariUI.FirstTimeTools;
        }
        else
        {
            panelButtons[6].onPointerDown.RemoveAllListeners();
            backImage.enabled = false;
        }
            
    }
}
