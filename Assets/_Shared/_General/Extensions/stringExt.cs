using UnityEngine;

public static class stringExt
{
    public static int PadLeft(this string[] list)
    {
        int count = list.Length;
        int maxLength = 0;
        for (int i = 0; i < count; i++)
            maxLength = Mathf.Max(maxLength, list[i].Length);
        
        for (int i = 0; i < count; i++)
            list[i] = list[i].PadLeft(maxLength);
        
        return maxLength;
    }
    
    public static int PadRight(this string[] list)
    {
        int count = list.Length;
        int maxLength = 0;
        for (int i = 0; i < count; i++)
            maxLength = Mathf.Max(maxLength, list[i].Length);
        
        for (int i = 0; i < count; i++)
            list[i] = list[i].PadRight(maxLength);
        
        return maxLength;
    }


    public static int ID(this string value)
    {
        return Shader.PropertyToID(value);
    }
}
