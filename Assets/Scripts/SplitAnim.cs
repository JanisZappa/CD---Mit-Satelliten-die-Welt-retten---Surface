using System;
using System.Collections;
using UnityEngine;


public class SplitAnim : Singleton<SplitAnim>
{
    public static float bottom, side, center; 
    public static float gameVis;
    public static float planetZoom;  
    private static int anims;

    public static bool Animating { get { return anims > 0; } }
    
    public static bool EarthView
    {
        get
        {
            return !Animating && bottom < .01f;
        }
    }
    public static bool GameView
    {
        get
        {
            return !Animating && bottom > .99f;
        }
    }
  

    private IEnumerator ValueAnim(bool show, float speed, Action<float> callback)
    {
        anims++;
        
        float start = show? 0 : 1;
        float end = show? 1 : 0;
        
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            callback.Invoke(Mathf.SmoothStep(start, end, t));
            yield return null;
        }
        
        anims--;
    }

    
    private readonly WaitForSeconds wfs = new WaitForSeconds(0);

    private IEnumerator GameBuildUp(bool show, Action callback)
    {
        const float sideSpeed = 2.5f;
        const float fadeSpeed = 1.25f;
        const float bottomSpeed = 1.75f;
        
        if (show)
        {
            StartCoroutine(ValueAnim(true, 1f / (2.0714285714285f - .25f) * 1.25f, f => planetZoom = f));
            yield return StartCoroutine(Wait());
            yield return StartCoroutine(Wait());
            yield return StartCoroutine(ValueAnim(true, fadeSpeed, f => gameVis = f));
            yield return StartCoroutine(ValueAnim(true, bottomSpeed, f => bottom  = f));
            yield return StartCoroutine(Wait());
                         StartCoroutine(ValueAnim(true, sideSpeed, f => side    = f));
            yield return StartCoroutine(ValueAnim(true, sideSpeed, f => center  = f));
        }
        else
        {
                         StartCoroutine(ValueAnim(false, sideSpeed, f => center  = f));
            yield return StartCoroutine(ValueAnim(false, sideSpeed, f => side    = f));
            
            yield return StartCoroutine(Wait());
            
            yield return StartCoroutine(ValueAnim(false, bottomSpeed, f => bottom  = f));
            StartCoroutine(ValueAnim(false, 1f / (2.0714285714285f - .25f), f => planetZoom = f));
            yield return StartCoroutine(ValueAnim(false, fadeSpeed, f => gameVis = f));
            
            DrawArea.HideGame();
        }
        
        callback?.Invoke();
    }


    private IEnumerator Wait()
    {
        anims++;
        yield return wfs;
        anims--;
    }


    public static void CloseGame(Action callback = null)
    {
        Inst.StartCoroutine(Inst.GameBuildUp(false, callback));
    }
    public static void ShowGame(Action callback = null)
    {
        Inst.StartCoroutine(Inst.GameBuildUp(true, callback));
    }
}
