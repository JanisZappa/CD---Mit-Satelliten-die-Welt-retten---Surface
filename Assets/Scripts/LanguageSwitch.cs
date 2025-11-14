using UnityEngine;
using UnityEngine.UI;

public class LanguageSwitch : MonoBehaviour
{
    public Sprite[] sprites;
    public static bool English;

    [Space] public SpriteLibrary sL;
    private Image image;
    
    private void Start()
    {
        if (Settings.GetValue("LanguageSwitch") < .01f)
        {
            gameObject.SetActive(false);
            return;
        }
        
        image = GetComponent<Image>();
        SuckButton button = GetComponent<SuckButton>();
        button.onPointerDown.AddListener(() =>
        {
            English = !English;
            image.sprite = sprites[English ? 0 : 1];
            if(English)
                sL.SetEnglish();
            else
                sL.SetGerman();
        });
    }
}
