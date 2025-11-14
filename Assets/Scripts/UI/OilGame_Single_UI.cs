using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class OilGame_Single_UI : GameUI
{
    public RectTransform tablet;
    
    public SuckButton toolA;
    public SuckButton toolB, x, back, next;
    [Space]
    public GameObject tA;
    public GameObject tB;

    private Image[][] sprites;
    
    private Transform bottomTools;

    private int state;
    private float[] stateAlphas;

    private SuckButton[] buttons;
    private SplitOilGame trap;
    
    public override void Init()
    {
        base.Init();
       
        sprites = new Image[3][];
        for (int i = 0; i < 3; i++)
            sprites[i] = images.transform.GetChild(i).GetComponentsInChildren<Image>();
        stateAlphas = new float[3];
        bottomTools = transform.GetChild(1);

        buttons = new[] { toolA, toolB, x, back, next};
    }
    
    
    public override void SetActive(bool show, SplitGame player1, SplitGame player2)
    {
        base.SetActive(show, player1, player2);
        trap = player1 as SplitOilGame;
        
        if (show)
        {
            StartCoroutine(ShowIt(3));
            
            state = 0;
            
            tA.SetActive(true);
            tB.SetActive(false);
            
            for (int i = 0; i < 5; i++)
                 buttons[i].SetActive(true);
            
            toolA.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel)
                {
                    player1.SetEraser(false);
                    tA.SetActive(true);
                    tB.SetActive(false);
                }
            });
            toolB.onPointerDown.AddListener(() =>
            {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel)
                {
                    player1.SetEraser(true);
                    tA.SetActive(false);
                    tB.SetActive(true);
                }
            });
            x.onPointerDown.AddListener( () => {
                if (SplitAnim.GameView && !PopUpManager.ShowingPanel)
                {
                    PopUpManager.Show(b =>
                    {
                        if(b == 1)
                            SplitAnim.CloseGame();
                    }, 10);
                }
            });
            
            back.onPointerDown.AddListener( () =>
                {
                    if (SplitAnim.GameView && state < 2 && !PopUpManager.ShowingPanel)
                    {
                        //Debug.Log("FDF" + state);
                        PopUpManager.Show(b =>
                        {
                            if(b == 1)
                                StateChange(false);
                        }, 7);
                    }
                }
            );
            next.onPointerDown.AddListener( () =>
                {
                    if (SplitAnim.GameView && state < 2 && !PopUpManager.ShowingPanel)
                    {
                        PopUpManager.Show(b =>
                        {
                            if(b == 1)
                                StateChange(true);
                        }, state == 0? 8 : 9);
                    }
                }
            );
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                buttons[i].onPointerDown.RemoveAllListeners();
                buttons[i].SetActive(false);
            }
            
            tablet.anchoredPosition = Vector2.one * 10000;
        }
    }


    private void StateChange(bool forward)
    {
        if (forward)
        {
            state++;
            player1.GameStart(state == 1);
            tA.SetActive(true);
            tB.SetActive(false);
            if (state == 2)
                SplitAnim.CloseGame();
        }
        else
        {
            player1.GameStart(false);
            tA.SetActive(true);
            tB.SetActive(false);
            state--;
        }
    }
    
    
    private void LateUpdate()
    {
        if (player1 != null)
        {
            Vector2 wP = player1.GetWindowPos();
            //TODO: 16 -> 13.5f
            tablet.anchoredPosition = new Vector3(NewUI.res.x * wP.x / 13.5f, NewUI.res.y * wP.y / 9f);

            if (state < 2)
                OilRatingUI.Scores[state] = trap.GetScore;
        }

        for (int i = 0; i < 3; i++)
        {
            float a = Mathf.Lerp(stateAlphas[i], state == i || i == 2 ? 1 : 0, Time.deltaTime * 20);
            stateAlphas[i] = a;
            NewUI.SetImageAlpha(sprites[i], SplitAnim.side * a);
        }
        
        float extra = (2560f - 2160) * .5f;
        bottomTools.localPosition = Vector3.up * ((images.activeInHierarchy? SplitAnim.center * 200 - 200 : -200) - extra);
    }
    
    
    private IEnumerator ShowIt(float wT = 3)
    {
        yield return new WaitForSeconds(wT);
        
        PopUpManager.Show(i => {
           
        }, 16);
    }
}
