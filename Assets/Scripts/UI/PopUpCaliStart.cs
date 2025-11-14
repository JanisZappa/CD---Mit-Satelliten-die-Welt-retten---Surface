using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpCaliStart : PopUp
{
    protected override void ButtonInit()
    {
        panelButtons[0].onPointerDown.AddListener( () =>
        {
            callback?.Invoke(0);
            ShowPopup();
        });
        panelButtons[1].onPointerDown.AddListener( () =>
        {
            callback?.Invoke(2);
            ShowPopup();
        });
        panelButtons[2].onPointerDown.AddListener( () =>
        {
            callback?.Invoke(1);
            ShowPopup();
        });
    }
}
