using System.Collections.Generic;
using System.IO;
using UnityEngine;


public static class Settings
{
    static Settings()
    {
        string[] rows = File.ReadAllText( Application.streamingAssetsPath + "/Settings.txt").Split('\n');
        
        floatMap = new Dictionary<string, float>();
        for (int i = 0; i < rows.Length; i++)
        {
            string r = rows[i];
            if(string.IsNullOrEmpty(r) || r.Length < 5)
                continue;
            
            string[] parts = r.Split(' ');
            floatMap.Add(parts[0], float.Parse(parts[1]));
        }
    }

    private static readonly Dictionary<string, float> floatMap;


    public static float GetValue(string name)
    {
        return floatMap.TryGetValue(name, out float v) ? v : 0;
    }
}
