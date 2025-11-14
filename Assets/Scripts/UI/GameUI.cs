using UnityEngine;

public class GameUI : MonoBehaviour
{
    protected SplitGame player1, player2;
    protected GameObject images;

    
    public virtual void Init()
    {
        images = transform.GetChild(0).gameObject;
    }
    

    public virtual void SetActive(bool show, SplitGame player1, SplitGame player2)
    {
        this.player1 = player1;
        this.player2 = player2;
        
        images.SetActive(show);
    }
}
