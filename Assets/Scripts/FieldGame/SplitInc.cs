using UnityEngine;


public class SplitInc : SplitGame
{
    [Space]
    public Texture2D[] textures;
    public Vector2[] ranges;
    
    private Texture2D merged;
    
    private Ink ink;
    
    private NewUI ui;
    private static readonly int calcMap  = Shader.PropertyToID("_CalcMap");
    private static readonly int infraRed = Shader.PropertyToID("_InfraRed");
    private static readonly int mainTex  = Shader.PropertyToID("_MainTex");
    

    private Texture2D TexGen(bool infrared = false, bool remap = false)
    {
        const int original = 512;
        int res = original;
        int size = res * 2;
        
        Texture2D tex = new Texture2D(size, size);
        
        int offset = infrared? 0 : 1;
        
        int i = 0;
        for (int y = 1; y > -1; y--)
        for (int x = 0; x < 2; x++)
            tex.SetPixels(x * res, y * res, res, res, textures[i++ * 2 + offset].GetPixels(0, 0, res, res));
        
        if (remap)
            HeatRemapper.InfraRemap(tex, ranges);
        
        tex.Apply();
        
        mat.SetTexture(name, tex);
        
        return tex;
    }
    

    public override void Init()
    {
        base.Init();
        
    //  SplitTex  //
        mat.SetTexture(mainTex, TexGen());
        mat.SetTexture(infraRed, TexGen(true));
        
        ink = GetComponent<Ink>();
        Texture2D calMap = TexGen(true, true);
        mat.SetTexture(calcMap, calMap);
        ink.Init(mat, calMap);
    }

    
    private void Update()
    {
        if(GameUpdate(out Vector4 cursorA))
            ink.UpdateCursors(cursorA, Input.GetKeyDown(KeyCode.Return));
    }
    

    public override SplitGame GameStart(bool activeWindow = true)
    {
        ink.UpdateCursors(Vector4.zero, true);
        
        return base.GameStart(activeWindow);
    }
    
    
    public Ink.Score GetScore => ink.score;
}
