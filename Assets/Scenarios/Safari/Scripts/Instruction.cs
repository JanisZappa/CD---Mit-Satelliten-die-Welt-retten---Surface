using TMPro;
using UnityEngine;


public class Instruction : MonoBehaviour
{
    public CarDetector detector;

    private TextMeshProUGUI text;
    private bool droning, charging;
    
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        TextUpdate();
    }

    private void Update()
    {
        if (droning != detector.droning || charging != detector.charging)
        {
            droning = detector.droning;
            charging = detector.charging;
            TextUpdate();
        }
    }


    private void TextUpdate()
    {
        string v = "";
        for (int i = 0; i < 7; i++)
            v += "Drücke " + i + " für " + ((SafariGame.GameType)i).ToString().Replace("_", " ") + "\n";
        
        v += "\n" + (droning? (charging? "Drohne läd auf" :  "Drohne ist aktiv"): "Drücke Leertaste für Drohne");

        text.text = v;
    }
    
}
