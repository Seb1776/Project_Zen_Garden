using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Data", menuName = "Plants/Plant Data", order = 0)]
public class PlantAsset : ScriptableObject 
{
    public string plantName;
    public GameWorlds appearsIn;
    public PlantQuality plantQuality;
    public int buyPrice;
    public int revenuePrice;
    public Vector3 initialScale;
}
