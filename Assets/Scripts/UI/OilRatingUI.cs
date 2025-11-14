using UnityEngine;

public class OilRatingUI : MonoBehaviour
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
        OilTrap.Score score = Scores[(int)type];
        rects[0].localScale = new Vector2( score.trappedSpills, s);
        rects[1].localScale = new Vector2(0, s);
    }


    public enum RatingType
    {
        Oil_SingleA, Oil_SingleB, Oil_MultiA, Oil_MultiB
    }


    public static readonly OilTrap.Score[] Scores = new OilTrap.Score[100];
}
