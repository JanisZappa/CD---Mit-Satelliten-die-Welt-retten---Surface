using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SafariUI : GameUI
{
    public SuckButton x, next, back, videoA, videoB;

    [Space] 
    public Image[] gamePadElephantSlots;
    public Sprite[] gamePadElephants;
    public Material chargeMaterial;
    public TextMeshProUGUI timeText;

    [Space] 
    public Image toolA;
    public Image toolB;
    public Sprite[] toolSprites;

    [Space] 
    public GameObject[] phase2Sprites;

    [Space] public float apsectOffset;
    [Space] 
    public RectTransform videoButton;
    private Vector2 vBPos;
        
    private static SplitSafariGame p1;
    
    private float[] stateAlphas;
    private SuckButton[] buttons;
    private Image[][] sprites;
    private RawImage[][] imgs;
    
    private Transform bottomTools;

    public static int Score => p1 == null? 0 : p1.Score;

    public static readonly bool[] SelectedTools = new bool[5];

    private int phase;
    
    
    public override void Init()
    {
        base.Init();
        
        sprites = new Image[3][];
        imgs    = new RawImage[3][];

        for (int i = 0; i < 3; i++)
        {
            sprites[i] = images.transform.GetChild(i).GetComponentsInChildren<Image>();
               imgs[i] = images.transform.GetChild(i).GetComponentsInChildren<RawImage>();
        }
            
        stateAlphas = new float[3];
        buttons     = new[] { x, next, back, videoA, videoB };
        
        bottomTools = transform.GetChild(1);

        vBPos = videoButton.anchoredPosition;
        Debug.Log(vBPos);
        videoButton.anchoredPosition = vBPos;
    }

    private bool hookedUp;
    private static readonly int Battery = Shader.PropertyToID("battery");
    public static bool FirstTimeTools;

    public override void SetActive(bool show, SplitGame player1, SplitGame player2)
    {
        phase = 0;
        videoButton.anchoredPosition = vBPos;
        
        base.SetActive(show, player1, player2);
        p1 = player1 as SplitSafariGame;
        if (p1 != null && !hookedUp)
        {
            hookedUp = true;
            p1.game.OnGameEnd += GameOnOnGameEnd;
            p1.game.DisableTexts();
        }

        if (show)
        {
            //Debug.Log("Showing Safari UI");

            FirstTimeTools = true;

            for (int i = 0; i < gamePadElephantSlots.Length; i++)
                gamePadElephantSlots[i].sprite = gamePadElephants[0];

            ShowIt();

            for (int i = 0; i < buttons.Length; i++)
                buttons[i].SetActive(true);

            x.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel)
                {
                    PopUpManager.Show(b =>
                    {
                        if (b == 1)
                            SplitAnim.CloseGame();
                    }, 21);
                }
            });

            next.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel && phase == 0)
                {
                    //Debug.Log("NextPhase");
                    p1.game.ForceGameOver();
                }
            });

            back.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel && phase == 1)
                {
                    //Debug.Log("Back to Tools");
                    FirstTimeTools = false;
                    PopUpManager.Show(i =>
                    {
                        switch (i)
                        {
                            case 0:
                                phase = 1;
                                videoButton.anchoredPosition = vBPos + Vector2.down * 185;
                                
                                toolA.sprite = toolSprites[SelectedTools[0] ? 0 : 1];
                                toolB.sprite = toolSprites[SelectedTools[2] ? 2 : SelectedTools[3] ? 3 : 4];

                                ToolSwipeSwap();

                                StartGameBasedOnTools();
                                break;
                        }
                    }, 19);
                }
            });

            videoA.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel && phase < 2)
                {
                    PopUpManager.Show(i =>
                    {
                        switch (i)
                        {
                            case 0:
                                Debug.Log("JoA");
                                break;
                        }
                    }, 22);
                }
            });

            videoB.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel && phase == 1)
                {
                    PopUpManager.Show(i =>
                    {
                        switch (i)
                        {
                            case 0:
                                Debug.Log("JoB");
                                break;
                        }
                    }, 22);
                }
            });
        }
        else
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].onPointerDown.RemoveAllListeners();
                buttons[i].SetActive(false);
            }
        }
    }

    private void GameOnOnGameEnd()
    {
        //Debug.Log("GameEnd");

        switch (phase)
        {
            case 0:
                PopUpManager.Show(i =>
                    {
                        switch (phase)
                        {
                            case 0:
                                ShowTools();
                                break;
                        }
            
                    }, 18);
                break;
            
            case 1:
                PopUpManager.Show(i =>
                {
                    switch (i)
                    {
                        case 0:
                            //Debug.Log("Banana");
                            SplitAnim.CloseGame();
                            break;
                
                        case 1:
                            phase = 0;
                            videoButton.anchoredPosition = vBPos;
                            //Debug.Log("Neustart");
                            ShowTools();
                            break;
                    }
                }, 20, 1);
                break;
                
        }
       
    }


    private void ShowTools()
    {
        //Debug.Log("ShowTools");
        SelectedTools[0] = true;
        SelectedTools[1] = false;
        
        SelectedTools[2] = true;
        SelectedTools[3] = false;
        SelectedTools[4] = false;
        
        PopUpManager.Show(i =>
        {
            switch (i)
            {
                case 0:
                    phase=1;
                    videoButton.anchoredPosition = vBPos + Vector2.down * 185;
                    
                    toolA.sprite = toolSprites[SelectedTools[0] ? 0 : 1];
                    toolB.sprite = toolSprites[SelectedTools[2] ? 2 : SelectedTools[3]? 3 : 4];

                    ToolSwipeSwap();
                    
                    StartGameBasedOnTools();
                    break;
            }
        }, 19, 1);
    }


    private void ToolSwipeSwap()
    {
        int selected = (SelectedTools[0] ? 0 : 3) +
                       (SelectedTools[2] ? 0 : SelectedTools[3] ? 1 : 2);

        int c = phase2Sprites.Length;
        for (int i = 0; i < c; i++)
            phase2Sprites[i].SetActive(i == selected);
    }
    

    private void StartGameBasedOnTools()
    {
        if (SelectedTools[0])
        {
            if(SelectedTools[2])
                p1.game.StartGame(SafariGame.GameType.Mobilfunk_Solo);
            if(SelectedTools[3])
                p1.game.StartGame(SafariGame.GameType.Mobilfunk_KI);
            if(SelectedTools[4])
                p1.game.StartGame(SafariGame.GameType.Mobilfunk_KI_Vernetzt);
        }
        else
        {
            if(SelectedTools[2])
                p1.game.StartGame(SafariGame.GameType.Satellit_Solo);
            if(SelectedTools[3])
                p1.game.StartGame(SafariGame.GameType.Satellit_KI);
            if(SelectedTools[4])
                p1.game.StartGame(SafariGame.GameType.Satellit_KI_Vernetzt);
        }
    }


    private void LateUpdate()
    {
        for (int i = 0; i < 3; i++)
        {
            float a = Mathf.Lerp(stateAlphas[i], phase == i || i == 2 ? 1 : 0, Time.deltaTime * 20);
            stateAlphas[i] = a;
            NewUI.SetImageAlpha(sprites[i], SplitAnim.side * a);
            NewUI.SetRawImageAlpha(imgs[i], SplitAnim.side * a);
        }

        float dist = Mathf.Lerp(300, 400, CamInfo.AdjustMulti);;
        bottomTools.localPosition = Vector3.up * ((images.activeInHierarchy? SplitAnim.center * dist - dist : -dist) + apsectOffset * CamInfo.AdjustMulti);

        if(p1 == null)
            return;
        
        for (int i = 0; i < gamePadElephantSlots.Length; i++)
            gamePadElephantSlots[i].sprite = gamePadElephants[i < Score? 1 : 0];
        
        chargeMaterial.SetFloat(Battery, p1.game.DroneCharge);
        
        timeText.text = TimeSpan.FromSeconds(Mathf.Round(p1.game.TimeLeft + .25f)).ToString(@"mm\:ss");
    }
    
    
    private void ShowIt(float wT = 3)
    {
        PopUpManager.Show(i => {
            switch (i)
            {
                case 0:
                    ChangeState();
                    break;
                
                case 1:
                    SplitAnim.CloseGame();
                    break;
            }
        }, 17, wT);
    }


    private void ChangeState()
    {
        p1.game.StartGame(SafariGame.GameType.Solo);
    }
}
