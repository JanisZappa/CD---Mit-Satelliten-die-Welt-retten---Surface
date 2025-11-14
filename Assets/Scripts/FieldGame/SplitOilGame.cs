using UnityEngine;


public class SplitOilGame : SplitGame
{
    private static readonly int Offset = Shader.PropertyToID("Offset");
    private OilTrap trap;
    private int myGameID;

    public override void Init()
    {
        base.Init();
        
        mat.SetFloat(Offset, Random.Range(-100f, 100f));

        trap = GetComponent<OilTrap>();
        trap.Init(mat, player1, player2);
    }
    
    
    public override SplitGame GameStart(bool activeWindow = true)
    {
        trap.Clear(myGameID != DrawArea.GameID);
        myGameID = DrawArea.GameID;
        
        return base.GameStart(activeWindow);
    }

    
    private void Update()
    {
        if (GameUpdate(out Vector4 cursorA))
            trap.GameUpdate(cursorA);
    }


    public OilTrap.Score GetScore => trap.result;
}
