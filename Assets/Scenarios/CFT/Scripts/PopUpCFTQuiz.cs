using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpCFTQuiz : PopUp
{
    public TextMeshProUGUI[] texts;
    public Image[] images;
    public Sprite[] spr;
    public Image weiterImg;
    private int question;
    private bool answered;
    
    private int[] qOrder = new int[5], aOrder = new int[3];
    
    public static readonly int[] Score = new int[3];

    public static int SelectedScenario =>
        Score[0] >= Score[1] ? (Score[0] >= Score[2] ? 0 : Score[1] >= Score[2] ? 1 : 2) : Score[1] >= Score[2] ? 1 : 2;
    
    
    
    protected override void ButtonInit()
    {
        panelButtons[0].onPointerDown.AddListener( () =>
        {
            callback?.Invoke(0);
            ShowPopup();
        });
        panelButtons[1].onPointerDown.AddListener( () =>
        {
            if (answered)
            {
                question++;
                if(question < 5)
                    SetQuestion();
                else
                {
                    callback?.Invoke(1);
                    ShowPopup();
                }
            }
        });
        
        panelButtons[3].onPointerDown.AddListener(() =>
        {
            if(!answered)
                Selected(0);
        });
        panelButtons[2].onPointerDown.AddListener( () =>
        {
            if(!answered)
                Selected(1);
        });
        panelButtons[4].onPointerDown.AddListener( () =>
        {
            if(!answered)
                Selected(2);
        });
    }


    private void RandomOrder(int[] list)
    {
        int count = list.Length;
        for (int i = 0; i < count; i++)
            list[i] = i;

        int count2 = count * 32;
        for (int i = 0; i < count2; i++)
        {
            int pA = Random.Range(0, count);
            int pB = Random.Range(0, count);
            int vA = list[pA];
            int vB = list[pB];

            list[pA] = vB;
            list[pB] = vA;
        }
    }


    private void OnEnable()
    {
        question = 0;
        answered = false;

        RandomOrder(qOrder);

        for (int i = 0; i < 3; i++)
            Score[i] = 0;
        
        SetQuestion();
    }


    private void Selected(int id)
    {
        images[id].sprite = spr[1];

        int v = CFTGame.GetQuestion(qOrder[question]).Answers[aOrder[id]].Value;
        
        Score[v]++;
        
        answered = true;
        weiterImg.color = new Color(1, 1, 1, 1);
        weiterImg.raycastTarget = true;
    }


    private void SetQuestion()
    {
        RandomOrder(aOrder);
        
        answered = false;
        weiterImg.color = new Color(1, 1, 1, .35f);
        weiterImg.raycastTarget = false;
        Question q = CFTGame.GetQuestion(qOrder[question]);

        if (LanguageSwitch.English)
        {
            texts[0].text = "Question " + (1 + question) + " of 5";
            texts[1].text = q.QText_EN;
        
            for (int i = 0; i < 3; i++)
                texts[2 + i].text = q.Answers[aOrder[i]].AText_EN;
            
            texts[5].text = "Your answer?";
        }
        else
        {
            texts[0].text = "Frage " + (1 + question) + " von 5";
            texts[1].text = q.QText;
        
            for (int i = 0; i < 3; i++)
                texts[2 + i].text = q.Answers[aOrder[i]].AText;

            texts[5].text = "Ihre Antwort?";
        }

        for (int i = 0; i < 3; i++)
            images[i].sprite = spr[0];
    }
}
