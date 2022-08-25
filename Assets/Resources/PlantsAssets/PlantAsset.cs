using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Plant Data", menuName = "Plants/Plant Data", order = 0)]
public class PlantAsset : ScriptableObject 
{
    public string plantName;
    public string plantDescription;
    public Plant uiPlant;
    public Sprite plantBackg;
    public GameWorlds appearsIn;
    public PlantQualityData plantQuality;
    public List<FlowerPotType> canBePlantedIn = new List<FlowerPotType>();
    public int unlockPrice;
    public int buyPrice;
    public int revenuePrice;
    public Vector3 initialScale;
}
