using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpCFTEnd : PopUp
{
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
}
