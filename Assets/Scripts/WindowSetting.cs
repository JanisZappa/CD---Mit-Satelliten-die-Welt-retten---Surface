public class WindowSetting : Singleton<WindowSetting>
{
    public float ww = 1.365f, wh = 1.028f, ww2 = 1.25f, wh2 = .775f;
    public float o;

    public static float WW  => Inst.ww;
    public static float WH  => Inst.wh;
    public static float WW2 => Inst.ww2;
    public static float WH2 => Inst.wh2;
    public static float O   => Inst.o;
}
