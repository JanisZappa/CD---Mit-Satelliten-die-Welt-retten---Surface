using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NeoUI : GameUI
{
    public NeoGame game;
    public Camera cam;
    public GameObject configWindow;

    [Space]
    public SuckButton x;
    public SuckButton next, back, l, r, a, b, c;
    
    [Space] 
    public SuckButton[] hButtons;
    public GameObject[] hInfo;
    public Image[] markers;
    public Sprite[] buttonSprites;
    public RectTransform topInfos, bottomInfos;
    
    
    private SuckButton[] buttons;
    private bool canInteract;

    public Image[] selection, items;
    public Sprite[] itemSprites;

    [Space] 
    public TextMeshProUGUI tmp ,tmp2;
    public GameObject[] phaseTwoButtons;
    
    [Space] 
    public GameObject flightData;
    public RectTransform flightTransform;
    public RectTransform[] bars;
    public Image[] barColors;
    public Texture2D barTex;

    public static int SAT, ORB, PU, PR, SP;
    public static int SELECT;
    public static float[] BARS = new float[3];
    public static int YEARS;
    
    private int phase;
    private float[] stateAlphas;
    private Image[][] sprites;

    private Vector3 lifeTimes;
    private bool sunInfo;
    
    
    private void Start()
    {
        ActivateConfigWindow(false);
        canInteract = false;
    }

    
    public override void Init()
    {
        base.Init();
        
        sprites = new Image[2][];

        for (int i = 0; i < 2; i++)
        {
            List<Image> collect = images.transform.GetChild(i).GetComponentsInChildren<Image>().ToList();
            int cnt = collect.Count;
            for (int e = 0; e < cnt; e++)
                if (collect[e].gameObject.CompareTag("FreeMarker"))
                {
                    collect.RemoveAt(e);
                    e--;
                    cnt--;
                }
            
            sprites[i] = collect.ToArray();
        }
         
            
        stateAlphas = new float[2];
        
        buttons = new[] { x, l, r, a, b, c, next, back, hButtons[0], hButtons[1], hButtons[2], hButtons[3] };
        
        SetPhase(0);
    }
    
    
    public override void SetActive(bool show, SplitGame player1, SplitGame player2)
    {
        if (show)
            SAT = 0;
        
        base.SetActive(show, player1, player2);

        canInteract = false;

        if (show)
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].SetActive(true);
            
            x.onPointerDown.AddListener(() =>
            {
                if (canInteract && !PopUpManager.ShowingPanel)
                {
                    CFT_Marker.DisableInfo();
                    PopUpManager.Show(b =>
                    {
                        if (b == 1)
                        {
                            ActivateConfigWindow(false);
                            SplitAnim.CloseGame();
                        }
                    }, 29);
                }
            });
            next.onPointerDown.AddListener(() =>
            {
                if (canInteract && !PopUpManager.ShowingPanel)
                {
                    switch (phase)
                    {
                        case 0:
                        {
                            SetPhase(1);
                            ActivateConfigWindow(false);
                            break;
                        }

                        case 1:
                        {
                            if (ORB >= 0 && ORB <= 2)
                            {
                                topInfos.gameObject.SetActive(false);
                                bottomInfos.gameObject.SetActive(false);
                                
                                flightData.SetActive(true);
                                lifeTimes = game.StartFlight();
                                canInteract = false;
                                for (int i = 0; i < 2; i++)
                                    phaseTwoButtons[i].SetActive(false);
                            }
                            break;
                        }
                    }
                }
            });
            back.onPointerDown.AddListener(() =>
            {
                if (canInteract && !PopUpManager.ShowingPanel && phase == 1)
                {
                    SetPhase(0);
                    ActivateConfigWindow(true);
                }
            });
            l.onPointerDown.AddListener(() =>
            {
                if (canInteract && !PopUpManager.ShowingPanel && phase == 0)
                {
                    game.LeftRight(true);
                    SAT = (SAT + 3 - 1) % 3;
                }
                    
            });
            r.onPointerDown.AddListener(() =>
            {
                if (canInteract && !PopUpManager.ShowingPanel && phase == 0)
                {
                    game.LeftRight(false);
                    SAT = (SAT + 3 + 1) % 3;
                } 
            });
            a.onPointerDown.AddListener(() =>
            {
                if (canInteract && !PopUpManager.ShowingPanel && phase == 0)
                {
                    UpdateSelection(0);
                    StartCoroutine(Delay());
                }   
            });
            b.onPointerDown.AddListener(() =>
            {
                if (canInteract && !PopUpManager.ShowingPanel && phase == 0)
                {
                    UpdateSelection(1);
                    StartCoroutine(Delay());
                }   
            });
            c.onPointerDown.AddListener(() =>
            {
                if (canInteract && !PopUpManager.ShowingPanel && phase == 0)
                {
                    UpdateSelection(2);
                    StartCoroutine(Delay());
                }   
            });

            for (int i = 0; i < 4; i++)
            {
                int it = i;
                hButtons[i].onPointerDown.AddListener(() =>
                {
                    if (canInteract && !PopUpManager.ShowingPanel && phase == 1)
                        SetOrbit(it);
                });
            }

            SetPhase(0);
            UpdateSelection(-1);
            PU = 0;
            PR = 0;
            SP = 0;
            
            UpdateItems();
            SetOrbit(0, true);
            StartCoroutine(WindowWait());
        }

        else
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].onPointerDown.RemoveAllListeners();
                buttons[i].SetActive(false);
            }
            
            ActivateConfigWindow(false);
        }
    }


    private void SetPhase(int p)
    {
        phase = p;
        game.SetPhase(p);
        flightData.SetActive(false);
        tmp.enabled = false;
        tmp2.enabled = false;

        sunInfo = false;
        
        for (int i = 0; i < 2; i++)
            phaseTwoButtons[i].SetActive(true);

        if (phase == 1)
        {
            topInfos.gameObject.SetActive(true);
            bottomInfos.gameObject.SetActive(true);
            
            SetOrbit(ORB);
        }
    }


    private void ShowConfigPopup()
    {
        PopUpManager.Show(b =>
        {
            switch (SELECT)
            {
                case 0:
                    PU = b;
                    UpdateItems();
                    break;
                case 1:
                    PR = b;
                    UpdateItems();
                    break;
                case 2:
                    SP = b;
                    UpdateItems();
                    break;
            }
            
        }, 28);
    }


    public void SetOrbit(int id, bool init = false)
    {
        if (init)
        {
            sunInfo = false;
            hInfo[3].SetActive(sunInfo);
        }
        
        if (id == 3)
        {
            sunInfo = !sunInfo;
            hInfo[3].SetActive(sunInfo);
            return;
        }
        
        ORB = id;
        for (int i = 0; i < 3; i++)
            hInfo[i].SetActive(i == id);

        for (int i = 0; i < 3; i++)
            markers[i].sprite = buttonSprites[i == id ? 1 : 0];
        game.SelectOrbit(id);
    }


    private void UpdateSelection(int s)
    {
        SELECT = s;
        for (int i = 0; i < 3; i++)
            selection[i].enabled = i == SELECT;
    }
    
    
    private void UpdateItems()
    {
        for (int i = 0; i < 3; i++)
        {
            int s = i == 0? PU : i == 1 ? PR : SP;
            items[i].sprite = itemSprites[i * 3 + s];
        }   
    }


    private IEnumerator WindowWait()
    {
        yield return null;
        while (SplitAnim.Animating)
            yield return null;
        
        ActivateConfigWindow(true);
        PopUpManager.Show(b =>
        {
            if (b == 0)
            {
                ActivateConfigWindow(false);
                SplitAnim.CloseGame();
            }
            else
            {
                canInteract = true;
            }
        }, 27);
    }

    
    private IEnumerator Delay()
    {
        yield return DelayTime;
        ShowConfigPopup();
    }
    

    private void ActivateConfigWindow(bool active)
    {
        configWindow.SetActive(active);
        game.ActivateConfigWindow(active);
    }

    public static readonly WaitForSeconds DelayTime = new WaitForSeconds(.25f);

    private void LateUpdate()
    {
        for (int i = 0; i < 2; i++)
        {
            float a = Mathf.Lerp(stateAlphas[i], phase == i || i == 2 ? 1 : 0, Time.deltaTime * 20);
            stateAlphas[i] = a;
            NewUI.SetImageAlpha(sprites[i], SplitAnim.side * a);
        }

        if (phase == 1)
        {
            topInfos.anchoredPosition    = GameViewToCanvas(game.PivotViewPos(true));
            bottomInfos.anchoredPosition = GameViewToCanvas(game.PivotViewPos(false));
        }
    }

    public void FlightUpdate(int years, float t, Vector3 p)
    {
        tmp.text = years.PrepString();
        
        flightTransform.anchoredPosition = GameViewToCanvas(p);

        if (t > 0)
        {
            for (int i = 0; i < 3; i++)
            {
                float life = i == 0 ? lifeTimes.x : i == 1 ? lifeTimes.y : lifeTimes.z;
                float w = Mathf.Max(0, life - t) / life;
                BARS[i] = w;
                SetBar(bars[i], barColors[i], w);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
                SetBar(bars[i], barColors[i], 1);
        }
    }


    private Vector3 GameViewToCanvas(Vector3 p)
    {
        p -= Vector3.one * .5f;
        p.x /= cam.aspect;
        p.x *= 3840;
        p.y *= Mathf.Lerp(2160, 2560, CamInfo.AdjustMulti);
        p.z = 0;

        return p;
    }


    public void SetBar(RectTransform bar, Image image, float w)
    {
        const float scaleWidth = .975f;
        const float minWidth = 1f - scaleWidth;
        w = float.IsNaN(w) ? 0 : w;
        bar.localScale = new Vector3(minWidth + w * scaleWidth, 1, 1);
        image.color = barTex.GetPixelBilinear(1f - Mathf.Pow(1f - w, 3), 0);
    }

    
    public void FlightStart()
    {
        tmp.enabled = true;
        tmp2.enabled = true;
        tmp2.text = LanguageSwitch.English ? "LIFETIME IN YEARS" : "LEBENSZEIT IN JAHREN";
        game.SelectOrbit(-1);
    }


    public void FlightOver(int years)
    {
        YEARS = years;
        
        tmp.text = years.PrepString();
        
        PopUpManager.Show(b =>
        {
            if (b == 0)
            {
                SetPhase(0);
                ActivateConfigWindow(true);
                canInteract = true;
            }
            else
            {
                SplitAnim.CloseGame();
            }
        }, 30);
    }
}
