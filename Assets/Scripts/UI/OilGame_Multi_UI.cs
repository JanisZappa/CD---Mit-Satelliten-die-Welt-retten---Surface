using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class OilGame_Multi_UI : GameUI
{
    public RectTransform tabletA;
    public RectTransform tabletB;
    
    public SuckButton tool0A;
    public SuckButton tool0B, tool1A, tool1B, x;
    [Space]
    public GameObject t0A;
    public GameObject t0B,t1A,t1B;
    [Space] 
    public GameObject win1;
    public GameObject win2;

    private int state;
    
    private Image[] sprites;

    private bool showAnim;

    private bool paused;
    
    private SuckButton[] buttons;
    
    private TimeTest[] times;
    private readonly float[] countTimes = new float[2];
    private readonly bool[] done = new bool[2];
    private float maxTime;
    private SplitOilGame p1, p2;
    private int winner;
    
    
    public override void Init()
    {
        base.Init();
        
        sprites = transform.GetChild(0).GetComponentsInChildren<Image>();
        times = GetComponentsInChildren<TimeTest>();
      
        buttons = new[] { tool0A, tool0B, tool1A, tool1B, x};
    }

    
    public override void SetActive(bool show, SplitGame player1, SplitGame player2)
    {
        base.SetActive(show, player1, player2);
        
        p1 = player1 as SplitOilGame;
        p2 = player2 as SplitOilGame;
        
        
        if (show)
        {
            GameReset(false);
            
            state = 0;
            
            t0A.SetActive(true);
            t0B.SetActive(false);
            t1A.SetActive(true);
            t1B.SetActive(false);
            
            
            
            for (int i = 0; i < 5; i++)
                buttons[i].SetActive(true);
            
            tool0A.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView)
                {
                    player1.SetEraser(false);
                    t0A.SetActive(true);
                    t0B.SetActive(false);
                }
            });
            tool0B.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView)
                {
                    player1.SetEraser(true);
                    t0A.SetActive(false);
                    t0B.SetActive(true);
                }
            });
            tool1A.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView)
                {
                    player2.SetEraser(false);
                    t1A.SetActive(true);
                    t1B.SetActive(false);
                }
            });
            tool1B.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView)
                {
                    player2.SetEraser(true);
                    t1A.SetActive(false);
                    t1B.SetActive(true);
                }
            });
            
            x.onPointerDown.AddListener( () => {
                if (SplitAnim.GameView && !showAnim && !PopUpManager.ShowingPanel)
                {
                    paused = true;
                    PopUpManager.Show(b =>
                    {
                        if (b == 1)
                        {
                            SplitAnim.CloseGame();
                            state = 0;

                        }
                            
                        paused = false;
                    }, 15);
                }
            });
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                buttons[i].onPointerDown.RemoveAllListeners();
                buttons[i].SetActive(false);
            }
            
            tabletA.anchoredPosition = Vector2.one * 10000;
            tabletB.anchoredPosition = Vector2.one * 10000;
        }
    }


    private void GameReset(bool again)
    {
        state = 0;
        
        maxTime = Settings.GetValue("Oil_Multiplayer-GameLength");
        win1.SetActive(false);
        win2.SetActive(false);
        
        for (int i = 0; i < 2; i++)
        {
            times[i].SetTime(0);
            countTimes[i] = 0;
            done[i] = false;
        }

        winner = -1;

        player1.GameStart();
        player2.GameStart();
        
        StartCoroutine(ShowIt(again? 1 : 3));
    }
    
    
    private void LateUpdate()
    {
        if (player1 != null)
        {
            Vector2 wP = player1.GetWindowPos();
            //TODO: 16 -> 13.5f
            tabletA.anchoredPosition = new Vector3(NewUI.res.x * wP.x / 13.5f, NewUI.res.y * wP.y / 9f);
        }
        if (player2 != null)
        {
            Vector2 wP = player2.GetWindowPos();
            //TODO: 16 -> 13.5f
            tabletB.anchoredPosition = new Vector3(NewUI.res.x * wP.x / 13.5f, NewUI.res.y * wP.y / 9f);
        }

        for (int i = 0; i < 2; i++)
        {
            times[i].SetColor(new Color(1, 1, 1, player1 != null? SplitAnim.side : 0));
        }
        
        
        NewUI.SetImageAlpha(sprites, SplitAnim.side);

        if (state == 1)
        {
            if (player1 != null && player2 != null)
            {
                if (!paused)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        OilTrap.Score s = i == 0 ? p1.GetScore : p2.GetScore;
                        if (Mathf.Approximately(s.trappedSpills, 1))
                        {
                            done[i] = true;
                            if (winner == -1)
                            {
                                winner = i;
                                (i == 0? win1 : win2).SetActive(true);
                            }  
                        }
                            

                        if (!done[i])
                        {
                            countTimes[i] = Mathf.Min(countTimes[i] + Time.deltaTime, maxTime);
                            times[i].SetTime(countTimes[i]);
                        }
                            
                    }
                    
                    if (done[0] && done[1] || countTimes[0] >= maxTime || countTimes[1] >= maxTime)
                    {
                        state = 2;
                        win1.SetActive(false);
                        win2.SetActive(false);
                
                        PopUpManager.Show(i => {
                            switch (i)
                            {
                                case 1 :
                                    SplitAnim.CloseGame();
                                    break;
                                case 0 :
                                    DrawArea.GameID++;
                                    GameReset(true);
                                    break;   
                            }
                        }, winner == -1? 14 : winner == 0? 12 : 13);
                    }
                }
                
                
                player1.DrawingBlocked = done[0];
                player2.DrawingBlocked = done[1];
                
                OilRatingUI.Scores[2] = p1.GetScore;
                OilRatingUI.Scores[3] = p2.GetScore;
            }
            else
            {
                state = 0;
            }
        }
        else
        {
            if(player1 != null)
                player1.DrawingBlocked = true;
            if(player2 != null)
                player2.DrawingBlocked = true;
        }
    }


    private IEnumerator ShowIt(float wT = 3)
    {
        showAnim = true;
        yield return new WaitForSeconds(wT);
        showAnim = false;
        PopUpManager.Show(i => {
            switch (i)
            {
                case 0 :
                    SplitAnim.CloseGame();
                    break;
                case 2 :
                    SplitAnim.CloseGame(EarthMenu.Show_Oil_Single_Please);
                    break;   
                case 1 :
                    state = 1;
                    break;   
            }
        }, 11);
    }
}
