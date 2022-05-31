using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Process", menuName = "Plants/Plant Process Data")]
public class PlantProcessAsset : ScriptableObject 
{
    public PlantQuality qualityToApply;
    public Vector2Int waterRange, compostRange, fertilizerRange, musicRange;
    public Vector2 timeRange;
}
