using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlantsManager : MonoBehaviour
{
    public enum GameWorlds 
    { 
        AncientEgypt, PirateSeas, WildWest, 
        FarFuture, DarkAges, BigWaveBeach, 
        FrostbiteCaves, LostCity, NeonMixtapeTour,
        JurassicMarsh, ModernDay
    }

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

    public GameObject GetSprout()
    {
        return sprout;
    }
}

[System.Serializable]
public class World
{
    public PlantsManager.GameWorlds world;
    public PlantAsset[] worldPlants;
}
