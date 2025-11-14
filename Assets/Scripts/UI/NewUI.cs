using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class NewUI : Singleton<NewUI>
{
    public GameUI[] gameUI;
    
    [Space]
    public GameObject overview;
    public static Vector2 res;
    
    private Image[] over;
    private bool twoPlayers;
    
    [Header("Overview")]
    public RectTransform[] marker;

    private Image[] markerImages;
    private float[] markerAlpha;
    public Image[] descriptions;
    private float[] dAlpha;
    private int markerCount, gameCount;
    

    public NewUI Init()
    {
        gameCount = gameUI.Length;
        for (int i = 0; i < gameCount; i++)
            gameUI[i].Init();
        
        over = overview.transform.GetComponentsInChildren<Image>();
        
        Button[] all = GetComponentsInChildren<Button>();
        for (int i = 0; i < all.Length; i++)
        {
            Button b = all[i];
            if(!b.gameObject.CompareTag("Finish"))
                b.GetComponent<Image>().color = new Color(0, 0, 0, 0);
			else
				Debug.Log(b.gameObject.name);
        } 
        
        HideWindows();
        
        dAlpha = new float[descriptions.Length];

        markerCount = marker.Length;
        markerImages = new Image[markerCount];
        for (int i = 0; i < markerCount; i++)
            markerImages[i] = marker[i].GetComponent<Image>();
        markerAlpha = new float[markerCount];
        
        return this;
    }
    
    
    public void ShowUI(SplitGame player1, SplitGame player2)
    {
        res = GetComponent<CanvasScaler>().referenceResolution;
        twoPlayers = player2 != null;
        GetUI(player1, player2).SetActive(true, player1, player2);
    }


    private GameUI GetUI(SplitGame player1, SplitGame player2)
    {
        switch (player1)
        {
            case SplitInc _:        return gameUI[twoPlayers ? 1 : 0];
            case SplitOilGame _:    return gameUI[twoPlayers ? 3 : 2];
            case SplitSafariGame _: return gameUI[4];
            case CFTGame _:         return gameUI[5];
            case NeoGame _:         return gameUI[6];
        }

        return null;
    }

    public void HideWindows()
    {
        for (int i = 0; i < gameCount; i++)
            gameUI[i].SetActive(false, null, null);
    }


    private void LateUpdate()
    {
        SetImageAlpha(over, 1 - SplitAnim.gameVis);
        SetDescriptionAlpha(1 - SplitAnim.gameVis);
        SetMarkerAlpha(1 - SplitAnim.gameVis);
    }


    public static void SetImageAlpha(Image[] images, float a)
    {
        int count = images.Length;
        for (int i = 0; i < count; i++)
        {
            Image img = images[i];
            Color c = img.color;
            c.a = a;
            img.color = c;
        }
    }
    
    
    public static void SetRawImageAlpha(RawImage[] images, float a)
    {
        int count = images.Length;
        for (int i = 0; i < count; i++)
        {
            RawImage img = images[i];
            Color c = img.color;
            c.a = a;
            img.color = c;
        }
    }
    
    
    private void SetDescriptionAlpha( float a)
    {
        int count = descriptions.Length;
        for (int i = 0; i < count; i++)
            descriptions[i].color = new Color(1, 1, 1, a * dAlpha[i] * markerAlpha[i]);
    }
    
    
    private void SetMarkerAlpha( float a)
    {
        for (int i = 0; i < markerCount; i++)
            markerImages[i].color = new Color(1, 1, 1, a * markerAlpha[i]);
    }

    
    public float UpdateMarker(int id, Vector2 viewPos, bool visible, float dot, bool hidden)
    {
        Vector2 p = viewPos - Vector2.one * .5f;
        p.x *= 3840;
        p.y *= Mathf.Lerp(2160, 2560, CamInfo.AdjustMulti);
        marker[id].anchoredPosition = p;
        marker[id].gameObject.SetActive(visible);
        marker[id].localRotation = Quaternion.AngleAxis(hidden? 45 : 0, Vector3.forward);

        float a = 1f - Mathf.Pow(1f - Mathf.Max(0, dot * 1.4f - .4f), 5);
        markerAlpha[id] = a;
        return a;
    }


    public void BestMarker(int best)
    {
        if (!fadingDescription && best != currentBestMarker)
            StartCoroutine(FadeDescriptions(best));
    }


    private int currentBestMarker = -1;
    private bool fadingDescription;


    private IEnumerator FadeDescriptions(int newBest)
    {
        fadingDescription = true;

        if (currentBestMarker != -1)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 4;

                dAlpha[currentBestMarker] = Mathf.SmoothStep(1, 0, t);

                yield return null;
            }
        }

        currentBestMarker = newBest;
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * 4;

                dAlpha[currentBestMarker] = Mathf.SmoothStep(0, 1, t);

                yield return null;
            }
        }

        fadingDescription = false;
    }
}
