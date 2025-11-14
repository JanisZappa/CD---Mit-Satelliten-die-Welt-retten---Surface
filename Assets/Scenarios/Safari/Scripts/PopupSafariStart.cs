using System;
using UnityEngine.UI;
using UnityEngine.Video;


public class PopupSafariStart : PopUp
{
    public VideoPlayer video;
    public VideoClip[] clips;
    private RawImage image;
    private Image videoButtonImage;

    
    public override void Init()
    {
        image = video.GetComponent<RawImage>();
        image.enabled = false;
        base.Init();
        videoButtonImage = panelButtons[panelButtons.Length - 1].GetComponent<Image>();
    }
    
    
    protected override void ButtonInit()
    {
        if(panelButtons.Length == 3)
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
            panelButtons[2].onPointerDown.AddListener(PlayVideo);
        }
        else
        {
            panelButtons[0].onPointerDown.AddListener(() =>
            {
                callback?.Invoke(0);
                ShowPopup();
            });
            panelButtons[1].onPointerDown.AddListener(PlayVideo);
        }
    }


    private void PlayVideo()
    {
        video.Play();
        image.enabled = true;
        videoButtonImage.enabled = false;
    }


    public override void ShowPopup(Action<int> callback = null)
    {
        base.ShowPopup(callback);
        image.enabled = false;
        video.clip = clips[LanguageSwitch.English ? 1 : 0];
        video.Stop();
    }
}
