using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameWorlds 
{ 
    AncientEgypt, PirateSeas, WildWest, 
    FarFuture, DarkAges, BigWaveBeach, 
    FrostbiteCaves, LostCity, NeonMixtapeTour,
    JurassicMarsh, ModernDay
}

public enum PlantQualityName
{
    Common, Rare, Epic, Legendary, Botanic
}

public enum GardenItemType
{
    Water, Compost, Fertilizer, Music
}

public class PlantsManager : MonoBehaviour
{
    [Header ("Plant Presets")]
    [SerializeField] private PlantProcessAsset[] plantProcessDatas;

    [SerializeField] private GameObject sprout;
    [SerializeField] private Plant debugPlant;
    [SerializeField] private FlowerPot debugFlowerPot;

    void Update()
    {
        if (Keyboard.current.nKey.wasPressedThisFrame)
        {
            GameObject _p = Instantiate(debugPlant, transform.position, Quaternion.identity).gameObject;
            debugFlowerPot.PlantPlant(_p.GetComponent<Plant>());
        }
    }

    public void AssignQualityData(Plant plant)
    {
        PlantProcessAsset ppa = null;

        for (int i = 0; i < plantProcessDatas.Length; i++)
        {
            if (plant.plantData.plantQuality == plantProcessDatas[i].qualityToApply)
            {
                ppa = plantProcessDatas[i];
                break;
            }
        }

        if (ppa != null)
            plant.SetPlantProgress(ppa);
    }

    public GameObject GetSprout()
    {
        return sprout;
    }
}

[System.Serializable]
public class World
{
    public GameWorlds world;
    public PlantAsset[] worldPlants;
}
