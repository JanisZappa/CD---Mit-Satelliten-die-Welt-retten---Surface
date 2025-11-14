using UnityEngine;

public class AndyIdle : MonoBehaviour
{
    public float idleThresh;
    [Space]
    public float idleTime;
    public static bool IdleMode;
    
    
    private void Start()
    {
        idleThresh = Settings.GetValue("TimeUntilIdle");
    }

    
    private void Update()
    {
        idleTime = !(Input.touchCount > 0 || Input.GetMouseButton(0))? idleTime + Time.deltaTime : 0;
        if (IdleMode)
        {
            if(idleTime <idleThresh)
                IdleMode = false;
        }
        else
        {
            if (idleTime > idleThresh)
            {
                IdleMode = true;
                if (SplitAnim.GameView)
                {
                    PopUpManager.HideActivePopup();
                    SplitAnim.CloseGame();
                }  
            }
        }
    }
}
