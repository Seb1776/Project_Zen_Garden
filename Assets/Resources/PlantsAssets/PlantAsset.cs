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
    public bool autoSettedPrice;
    [NonReorderable]
    public PlantLevel[] plantLevels;
    public Vector3 initialScale;

    public PlantLevel GetPlantLevel(int idx)
    {
        for (int i = 0; i < plantLevels.Length; i++)
            if (idx == i)
                return plantLevels[i];

        return null;
    }
}

[System.Serializable]
public class PlantLevel
{
    public int upgradePrice;
    public int producedCoins;
    public int producingTime;
    public int plantLife;
    public int energyLevel;
    public int sellPrice;
}
