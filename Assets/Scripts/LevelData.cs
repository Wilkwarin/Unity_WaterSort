using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "WaterSort/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public int levelNumber;
    public string levelName = "Level 1";

    [Header("Bottle Configuration")]
    public BottleConfiguration[] bottles;
    
    [System.Serializable]
    public class BottleConfiguration
    {
        public Color[] colors = new Color[4];
        [Range(0, 4)]
        public int numberOfColors = 4;
    }
}