using UnityEngine;
using UnityEngine.UI;


public class CFTUI : GameUI
{
    public CFTGame game;
    public RectTransform tablet;
    
    public SuckButton x, next, back, s1, s2;

    [Space] 
    public Image step1;
    public Sprite[] step1Sprites;
    
    [Space] 
    public Image step2;
    public Sprite[] step2Sprites;

    [Space] 
    public CFT_Marker[] markers;
    public Sprite[] markerSprites;
    
    
    private int phase;
    
    private Image[][] sprites;
    private SuckButton[] buttons;
    private float[] stateAlphas;
    public static int Scenario;

    private static int LangScen => Scenario + (LanguageSwitch.English ? 3 : 0);
    
    
    public override void Init()
    {
        base.Init();
        
        sprites = new Image[2][];

        for (int i = 0; i < 2; i++)
            sprites[i] = images.transform.GetChild(i).GetComponentsInChildren<Image>();
            
        stateAlphas = new float[2];
        buttons     = new[] { x, next, back, s1, s2 };
        
        tablet.anchoredPosition = Vector2.one * 10000;

        markers = GetComponentsInChildren<CFT_Marker>();
    }
    
    
    public override void SetActive(bool show, SplitGame player1, SplitGame player2)
    {
        phase = 0;
        Scenario = 0;
        
        base.SetActive(show, player1, player2);
        

        if (show)
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].SetActive(true);
            
            ShowIt();

            x.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel)
                {
                    CFT_Marker.DisableInfo();
                    PopUpManager.Show(b =>
                    {
                        if (b == 1)
                            SplitAnim.CloseGame();
                    }, 26);
                }
            });

            

            next.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel && phase == 1)
                {
                    CFT_Marker.DisableInfo();
                    PopUpManager.Show(b =>
                    {
                        switch (b)
                        {
                            default:
                                phase = 0;
                                game.StartFresh();
                                ShowIt();
                                break;
                            
                            case 1:
                                SplitAnim.CloseGame();
                                break;
                            
                            case 2:
                            case 3:
                            case 4:
                                Scenario = b - 2;
                                step2.sprite = step2Sprites[LangScen];
                                game.ShowVideo(Scenario);
                                break;
                        }
                    }, 25);
                }
            });

            
            back.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel && phase == 1)
                {
                    CFT_Marker.DisableInfo();
                    phase = 0;
                    game.StartFresh();
                    ShowIt();
                }
            });

            s1.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel && phase == 1)
                {
                    CFT_Marker.DisableInfo();
                    switch (Scenario)
                    {
                        case 0:
                        {
                            Scenario = 1;
                            step2.sprite = step2Sprites[LangScen];
                            game.ShowVideo(Scenario);
                        }
                        break;
                        case 1:
                        {
                            Scenario = 0;
                            step2.sprite = step2Sprites[LangScen];
                            game.ShowVideo(Scenario);
                        }
                            break;
                        case 2:
                        {
                            Scenario = 0;
                            step2.sprite = step2Sprites[LangScen];
                            game.ShowVideo(Scenario);
                        }
                            break;
                    }
                }
            });

            s2.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel && phase == 1)
                {
                    CFT_Marker.DisableInfo();
                    switch (Scenario)
                    {
                        case 0:
                        {
                            Scenario = 2;
                            step2.sprite = step2Sprites[LangScen];
                            game.ShowVideo(Scenario);
                        }
                            break;
                        case 1:
                        {
                            Scenario = 2;
                            step2.sprite = step2Sprites[LangScen];
                            game.ShowVideo(Scenario);
                        }
                            break;
                        case 2:
                        {
                            Scenario = 1;
                            step2.sprite = step2Sprites[LangScen];
                            game.ShowVideo(Scenario);
                        }
                            break;
                    }
                }
            });

            step1.sprite = step1Sprites[LanguageSwitch.English ? 1 : 0];
        }
        else
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].onPointerDown.RemoveAllListeners();
                buttons[i].SetActive(false);
            }
            
            tablet.anchoredPosition = Vector2.one * 10000;
        }
    }

    
    private void LateUpdate()
    {
        if (player1 != null)
        {
            Vector2 wP = player1.GetWindowPos();
            Vector3 aP = new Vector3(NewUI.res.x * wP.x / 16f, NewUI.res.y * wP.y / 9f);
            aP = new Vector3(NewUI.res.x * wP.x / 13.5f, NewUI.res.y * wP.y / 9f);
            tablet.anchoredPosition = aP;

            if (phase == 1)
            {
                int mC = markers.Length;
                for (int i = 0; i < mC; i++)
                    markers[i].MarkerUpdate(aP, markerSprites);
            }

            
        }
        
        for (int i = 0; i < 2; i++)
        {
            float a = Mathf.Lerp(stateAlphas[i], phase == i || i == 2 ? 1 : 0, Time.deltaTime * 20);
            stateAlphas[i] = a;
            NewUI.SetImageAlpha(sprites[i], SplitAnim.side * a);
        }
    }
    
    
    private void ShowIt(float wT = 3)
    {
        PopUpManager.Show(i => {
            switch (i)
            {
                case 0:
                    SplitAnim.CloseGame();
                    break;
                
                case 1:
                    ShowResultPopUp();
                    break;
            }
        }, 23, wT);
    }
    


    private void ShowResultPopUp()
    {
        PopUpManager.Show(i => {
            switch (i)
            {
                case 0:
                    SplitAnim.CloseGame();
                    break;
                
                case 1:
                    phase = 1;
                    Scenario = PopUpCFTQuiz.SelectedScenario;
                    step2.sprite = step2Sprites[LangScen];
                    game.ShowVideo(Scenario);
                    break;
            }
        }, 24, 1);
    }
}
