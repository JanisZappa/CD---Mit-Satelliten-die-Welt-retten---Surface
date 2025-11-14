using UnityEngine;

[CreateAssetMenu]
public class SafariSettings : ScriptableObject
{
    public GameSettings[] settings;
    
    [System.Serializable]
    public struct GameSettings
    {
        public int elephants, zebras;
        public bool receivers;
        public bool activeMarkers;
        public Detection certainty;
    }
    
    
    public enum Detection  { None, Maybe, Certain }
}
