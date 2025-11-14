using System;
using UnityEngine;

public class PopUp : MonoBehaviour
{
    public SuckButton[] panelButtons;
    protected Action<int> callback;
    private GameObject sprites;


    private static PopUp active;
    
    public virtual void Init()
    {
        gameObject.SetActive(true);
        sprites = transform.GetChild(0).gameObject;
        panelButtons = transform.GetComponentsInChildren<SuckButton>();
        ButtonInit();
        
        ShowPopup();
    }
    
    
    protected virtual void ButtonInit()
    {
        for (int i = 0; i < panelButtons.Length; i++)
        {
            int i1 = i;
            panelButtons[i].onPointerDown.AddListener( () =>
            {
                callback?.Invoke(i1);
                ShowPopup();
            });
        }
    }
    
    
    public virtual void ShowPopup(Action<int> callback = null)
    {
        this.callback = callback;
        bool show = callback != null;
        gameObject.SetActive(show);
        sprites.SetActive(show);

        for (int i = 0; i < panelButtons.Length; i++)
            panelButtons[i].SetActive(show);

        active = show ? this : null;
    }


    public static bool HideActivePopup()
    {
        if (active)
        {
            active.ShowPopup();
            return true;
        }
           
        return false;
    }
    
    
}
