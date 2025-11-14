using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class CaliforniaUI : GameUI
{
    public RectTransform tabletA;
    public RectTransform tabletB;
    
    public SuckButton tool0A;
    public SuckButton tool0B, tool1A, tool1B, x;
    [Space]
    public GameObject t0A;
    public GameObject t0B,t1A,t1B;

    private int state;
    
    private Image[] sprites;

    private bool showAnim;

    private TimeTest time;
    private float countTime;

    private WasserAmount waterAmount;
    private readonly FloatForce waterLevel = new FloatForce(200).SetSpeed(700).SetDamp(33);
    private float level;
    private bool paused;
    
    private SuckButton[] buttons;
    private SplitInc p1, p2;
    
    public override void Init()
    {
        base.Init();
        
        sprites = transform.GetChild(0).GetComponentsInChildren<Image>();
        time = GetComponentInChildren<TimeTest>();
        waterAmount = GetComponentInChildren<WasserAmount>();
        
        buttons = new[] { tool0A, tool0B, tool1A, tool1B, x};
    }

    
    public override void SetActive(bool show, SplitGame player1, SplitGame player2)
    {
        base.SetActive(show, player1, player2);
        p1 = player1 as SplitInc;
        p2 = player2 as SplitInc;
        
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
                    }, 6);
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
            
            RatingUI.Scores[2] = new Ink.Score
            {
                maskHitRatio   = 0,
                wasteAreaRatio = 0
            };
            
            Debug.Log("Hiding California");
        }
    }


    private void GameReset(bool again)
    {
        state = 0;
        level = 1;
        countTime = Settings.GetValue("California-GameLength");
        time.SetTime(countTime);

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
        
        time.SetColor(new Color(1, 1, 1, player1 != null? SplitAnim.side : 0));
        
        NewUI.SetImageAlpha(sprites, SplitAnim.side);

        if (state == 1)
        {
            if (player1 != null && player2 != null)
            {
                if (!paused)
                {
                    countTime -= Time.deltaTime;
                    if (countTime <= 0)
                    {
                        countTime = 0;
                        state = 2;
                
                        PopUpManager.Show(i => {
                            switch (i)
                            {
                                case 1 :
                                    SplitAnim.CloseGame();
                                    break;
                                case 0 :
                                    GameReset(true);
                                    break;   
                            }
                        }, 5);
                    }
                }
            

                Ink.Score s1 = p1.GetScore, s2 = p2.GetScore;
                RatingUI.Scores[2] = new Ink.Score
                {
                    maskHitRatio   = s1.maskHitRatio * .5f + s2.maskHitRatio * .5f,
                    wasteAreaRatio = s1.wasteAreaRatio * .5f + s2.wasteAreaRatio * .5f
                };
            
                time.SetTime(countTime);

                float dropped = s1.droppedWater + s2.droppedWater;
                float w = dropped > .01f?  1 - Mathf.Clamp01(dropped * .8f / (s1.mask + s2.mask)) : 1;
                if (w <= 0)
                {
                    player1.DrawingBlocked = true;
                    player2.DrawingBlocked = true;
                }
                else
                {
                    player1.DrawingBlocked = false;
                    player2.DrawingBlocked = false;
                }

                level = Mathf.Clamp01(waterLevel.Update(w, Time.deltaTime));
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
        
        waterAmount.SetAmount(level);
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
                    SplitAnim.CloseGame(EarthMenu.ShowBerryPlease);
                    break;   
                case 1 :
                    state = 1;
                    break;   
            }
        }, 4);
    }
}
