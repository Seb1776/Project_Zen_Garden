using UnityEngine;

[CreateAssetMenu(fileName = "New Quality Data", menuName = "Plants/Plant Quality")]
public class PlantQualityData : ScriptableObject 
{
    public PlantQualityName quality;
    public Color qualityColor;
}
