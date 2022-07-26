using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Process", menuName = "Plants/Plant Process Data")]
public class PlantProcessAsset : ScriptableObject 
{
    public PlantQualityData qualityToApply;
    public Vector2Int waterRange, compostRange, fertilizerRange, musicRange;
    public Vector2 timeRange;
    [Range(0f, 150f)]
    public float plantPercentageExtra;
}
