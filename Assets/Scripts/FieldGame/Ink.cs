using UnityEngine;


public class Ink : MonoBehaviour
{
    private int size;
    public ComputeShader compute;
    
    public RenderTexture rT;
    private int init, add, update, cursorupdate, count;
    
    private ComputeBuffer args, cursorArgs, counterBuffer;
    private int[] read, reset;
    
    private const int subSteps = 32;

    [Space] public Score score;
    
    private Material mat;
    private static readonly int water = Shader.PropertyToID("_Water");


    public void Init(Material mat, Texture2D mask)
    {
        size = mask.width;
        // Debug.Log(size);
        
        rT = new RenderTexture(size, size, 0) {enableRandomWrite = true, filterMode = FilterMode.Trilinear};
        rT.Create();
        rT.name = name;
        mat.SetTexture(water, rT);
        
        compute = Instantiate(compute);
        
        init   = compute.FindKernel("init");
        add    = compute.FindKernel("add");
        update = compute.FindKernel("update");
        count  = compute.FindKernel("count");
            
        compute.SetInt("size", size);
        args = Buff.Add(new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments).Init(new[]{ Mathf.CeilToInt(size / 16f), Mathf.CeilToInt(size / 16f), 1, 0}));
        ComputeBuffer a       = Buff.Add(new ComputeBuffer(size * size, 4));
        ComputeBuffer smoothA = Buff.Add(new ComputeBuffer(size * size, 4));
        ComputeBuffer vel     = Buff.Add(new ComputeBuffer(size * size, 4));
        
        //  Cursor  //
        cursorupdate = compute.FindKernel("cursorupdate");
        cursorArgs = Buff.Add(new ComputeBuffer(1, 16, ComputeBufferType.IndirectArguments).Init(new[]{1, 1, 1, 0}));
        ComputeBuffer b = Buff.Add(new ComputeBuffer(subSteps * 2, 16));
        compute.SetBuffer(cursorupdate, "brushes", b);
        ComputeBuffer c = Buff.Add(new ComputeBuffer(1, 6 * 4 * 2));
        compute.SetBuffer(cursorupdate, "cursorData", c);
        
        counterBuffer = Buff.Add(new ComputeBuffer(3, 4));
        compute.SetBuffer(count, "ValueCounter", counterBuffer);
        read = new int[3];
        reset = new int[3];
        
        compute.SetTexture(count, "mask", mask);
        
        KernelInit(init,   a, smoothA, vel, b);
        KernelInit(add,    a, smoothA, vel, b);
        KernelInit(update, a, smoothA, vel, b);
        KernelInit(count,  a, smoothA, vel, b);
        
        
        compute.DispatchIndirect(init, args);
        CountIt();
    }


    private void KernelInit(int kernel, ComputeBuffer a, ComputeBuffer smoothA, ComputeBuffer vel, ComputeBuffer b)
    {
        compute.SetTexture(kernel, "rT", rT);
        compute.SetBuffer( kernel, "values",  a);
        compute.SetBuffer( kernel, "smoothvalues",  smoothA);
        compute.SetBuffer( kernel, "vel",  vel);
        compute.SetBuffer( kernel, "brushes",  b);
    }


    private void CountIt()
    {
        counterBuffer.SetData(reset);
        compute.DispatchIndirect(count, args);
        counterBuffer.GetData(read);
        score.Calculate(read);
    }


    public void UpdateCursors(Vector4 a, bool clear = false)
    {
        compute.SetVector("cursor", a);
        
        compute.SetFloat("dt", Time.deltaTime);

        compute.DispatchIndirect(cursorupdate, cursorArgs);
        
        if(clear)
            compute.DispatchIndirect(init, args);
            
        compute.DispatchIndirect(add, args);
        
        //compute.DispatchIndirect(cursorupdate, cursorArgs);
        //compute.DispatchIndirect(add, args);
        
        CountIt();
        
        compute.DispatchIndirect(update, args);
    }
    
    
    public int Size { get { return size; }}
    
    [System.Serializable]
    public struct Score
    {
        public float maskHitRatio;
        public float wasteAreaRatio;
        public float wasteWaterRatio;
        public float rating;
        public float mask, droppedWater;


        public void Calculate(int[] read)
        {
            droppedWater = read[0];
            int maskHitWater = read[1];
            mask         = read[2];

            const int total = 1024 * 1024 * 256;
        
            maskHitRatio    = maskHitWater * 1f / mask;
            wasteAreaRatio  = (droppedWater - maskHitWater) * 1f / (total - mask);
            wasteWaterRatio = droppedWater == 0? 0 : 1f - maskHitWater * 1f / droppedWater;
            rating = Mathf.Clamp01(maskHitRatio - (droppedWater - maskHitWater) * 1f / mask); 
        }
    }
}
