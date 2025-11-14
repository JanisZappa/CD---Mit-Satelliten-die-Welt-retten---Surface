using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NeoResultPopUp : PopUp
{
    public NeoUI neoui;
    
    public RectTransform[] bars;
    public Image[] barColors;

    public TextMeshProUGUI tmp;

    public TextMeshProUGUI[] names;

    private string[] n = { "RADIATION HARD", "COTS", "GETESTETED COTS", "RADIATION HARD", "COTS", "TESTED COTS" };
    
    public override void ShowPopup(Action<int> callback = null)
    {
        base.ShowPopup(callback);

        for (int i = 0; i < 3; i++)
            neoui.SetBar(bars[i], barColors[i], NeoUI.BARS[i]);

        tmp.text = NeoUI.YEARS.PrepString();

        names[0].text = n[NeoUI.PU + (LanguageSwitch.English? 3 : 0)];
        names[1].text = n[NeoUI.PR + (LanguageSwitch.English? 3 : 0)];
        names[2].text = n[NeoUI.SP + (LanguageSwitch.English? 3 : 0)];
    }
}
