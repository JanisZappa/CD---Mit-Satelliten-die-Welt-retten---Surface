using TMPro;
using UnityEngine;


public class TimeTest : MonoBehaviour
{
    private TextMeshProUGUI text;
    
    private void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    public void SetTime(float seconds)
    {
        int m = Mathf.Min(Mathf.FloorToInt(seconds / 60), 59);
        int s = Mathf.Min(Mathf.RoundToInt(seconds - m * 60), 59);

        text.text = m.ToString().PadLeft(2, '0') + ":" + s.ToString().PadLeft(2, '0');
    }

    public void SetColor(Color color)
    {
        text.color = color;
    }
}
