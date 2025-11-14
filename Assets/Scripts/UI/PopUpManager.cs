using System;
using System.Collections;
using UnityEngine;


public class PopUpManager : Singleton<PopUpManager>
{
    public PopUp[] popups;
    
    public static bool ShowingPanel, ShowingPopUp;

    private static Action<int> callback;


    private void Start()
    {
        for (int i = 0; i < popups.Length; i++)
            popups[i].Init();
    }
    

    public static void Show(Action<int> callback, int what, float delay = 0)
    {
        //if(ShowingPanel)
          //  return;
        
        ShowingPanel = true;
        PopUpManager.callback = callback;

        Inst.StartCoroutine(Inst.DelayedShow(what, delay));
    }


    private IEnumerator DelayedShow(int what, float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        ShowingPopUp = true;
        Inst.popups[what].ShowPopup(b =>
        {
            ShowingPanel = false;
            ShowingPopUp = false;
            callback?.Invoke(b);
        });
    }


    public static void HideActivePopup()
    {
        PopUp.HideActivePopup();
        ShowingPanel = false;
        ShowingPopUp = false;
    }
}
