using UnityEngine;


public class HeatRemapper : Singleton<HeatRemapper>
{
    public Texture2D heatRamp;
    public ComputeShader compute;


    public static void InfraRemap(Texture2D tex, Vector2[] ranges)
    {
        Color[] pixels = tex.GetPixels();
        Color[] compare = Inst.heatRamp.GetPixels();

        int kernel = Inst.compute.FindKernel("CSMain");

        ComputeBuffer buff = Buff.New(pixels, 4 * 4);
        Inst.compute.SetBuffer(kernel, "Pixels", buff);
        Inst.compute.SetBuffer(kernel, "Ramp", Buff.New(compare, 4 * 4));
        Inst.compute.SetBuffer(kernel, "Ranges", Buff.New(ranges, 2 * 4));

        int width = tex.width;
        Inst.compute.SetInt("width", width);
        Inst.compute.SetInt("count", compare.Length);

        ComputeBuffer args = new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments).Init(new[] {width / 16, width / 16, 1, 0});
        Inst.compute.DispatchIndirect(kernel, args);
        buff.GetData(pixels);

        tex.SetPixels(pixels);

        buff.Dispose();
        args.Dispose();
    }
}