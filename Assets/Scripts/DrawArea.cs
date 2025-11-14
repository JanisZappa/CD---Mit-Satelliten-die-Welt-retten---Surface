using UnityEngine;


public class DrawArea : Singleton<DrawArea>
{
    public bool scaleAnim;
    private SplitGame[] splitGames;
    
    private NewUI ui;
    
    private static string currentGame;
    public static int GameID;
    

    private void Start()
    {
        splitGames = GetComponentsInChildren<SplitGame>();
        int gameCount = splitGames.Length;
        for (int i = 0; i < gameCount; i++)
            splitGames[i].Init();
        
        ui = FindObjectOfType<NewUI>().Init();
    }


    public static void ShowGame(string gamename)
    {
        Inst.GameStart(gamename);
    }
    

    private void GameStart(string gamename)
    {
        Debug.Log("Start Game: " + gamename);
        GameID++;
        switch (gamename)
        {
            case "Berry":
                ui.ShowUI(splitGames[0].GameStart(false), null);
                break;
            
            case "California":
                ui.ShowUI(splitGames[1].GameStart(), splitGames[2].GameStart());
                break;
            
            case "Oil_Single":
                ui.ShowUI(splitGames[3].GameStart(false), null);
                break;
            
            case "Oil_Multi":
                ui.ShowUI(splitGames[4].GameStart(), splitGames[5].GameStart());
                break;
            
            case "Safari":
                ui.ShowUI(splitGames[6].GameStart(), null);
                break;
            
            case "CFT":
                ui.ShowUI(splitGames[7].GameStart(), null);
                break;
            
            case "Neo":
                ui.ShowUI(splitGames[8].GameStart(), null);
                break;
            
            default:
                Debug.Log("Game is not Setup");
                break;
        }
        
        currentGame = gamename;
    }


    public static void HideGame()
    {
        if(currentGame != "")
            Inst.GameStop();
    }
    
    
    private void GameStop()
    {
        switch (currentGame)
        {
            case "Berry":
                splitGames[0].GameStop();
                break;
            
            case "California":
                splitGames[1].GameStop();
                splitGames[2].GameStop();
                break;
            
            case "Oil_Single":
                splitGames[3].GameStop();
                break;
            
            case "Oil_Multi":
                splitGames[4].GameStop();
                splitGames[5].GameStop();
                break;
            
            case "Safari":
                splitGames[6].GameStop();
                break;
            
            case "CFT":
                splitGames[7].GameStop();
                break;
            
            case "Neo":
                splitGames[8].GameStop();
                break;
        }
        
        currentGame = "";
        
        ui.HideWindows();
    }

    private bool showMap;
    private void LateUpdate()
    {
        transform.localScale = 
            Vector3.one * (scaleAnim? Mathf.Lerp(.25f, 1, Mathf.Pow(SplitAnim.planetZoom, 2)) : 1);

        if (Input.GetKeyDown(KeyCode.O))
        {
            showMap = !showMap;
            
            Shader.EnableKeyword( showMap? "SHOWMAP_ON" : "SHOWMAP_OFF");
            Shader.DisableKeyword(!showMap? "SHOWMAP_ON" : "SHOWMAP_OFF");
        }
    }
}
