using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NeoConfigPopup : PopUp
{
    public GameObject[] panels;
    public Image[] selection;
    public Image[] items;
    public Sprite[] itemSprites;
    
    
    protected override void ButtonInit()
    {
        for (int i = 0; i < panelButtons.Length; i++)
        {
            int i1 = i;
            panelButtons[i].onPointerDown.AddListener( () =>
            {
                UpdateSelection(i1);
                StartCoroutine(Delay(i1));
            });
        }
    }


    public override void ShowPopup(Action<int> callback = null)
    {
        base.ShowPopup(callback);

        UpdateSelection(NeoUI.SELECT == 0? NeoUI.PU : NeoUI.SELECT == 1? NeoUI.PR : NeoUI.SP);

        for (int i = 0; i < 3; i++)
            items[i].sprite = itemSprites[i + NeoUI.SELECT * 3];
        
        for (int i = 0; i < 3; i++)
            panels[i].SetActive(i == NeoUI.SELECT);
    }
    
    private void UpdateSelection(int s)
    {
        for (int i = 0; i < 3; i++)
            selection[i].enabled = i == s;
    }

    private IEnumerator Delay(int i1)
    {
        yield return NeoUI.DelayTime;
        callback?.Invoke(i1);
        ShowPopup();
    }
}
