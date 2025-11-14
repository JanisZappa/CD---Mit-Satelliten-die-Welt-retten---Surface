using UnityEngine;

public class RatingUI : MonoBehaviour
{
    public RatingType type;
    private RectTransform[] rects;
    private float s;
    
    private void OnEnable()
    {
        if (rects == null)
        {
            rects = new RectTransform[2];
            for (int i = 0; i < 2; i++)
                rects[i] = transform.GetChild(i).GetComponent<RectTransform>();

            s = rects[0].localScale.y;
        }
    }


    private void LateUpdate()
    {
        Ink.Score score = Scores[(int)type];
        rects[0].localScale = new Vector2( score.maskHitRatio, s);
        rects[1].localScale = new Vector2( score.wasteAreaRatio, s);
    }


    public enum RatingType
    {
        BerryA, BerryB, California
    }


    public static readonly Ink.Score[] Scores = new Ink.Score[100];
}
