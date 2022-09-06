using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameWorlds 
{ 
    AncientEgypt, PirateSeas, WildWest, 
    FarFuture, DarkAges, BigWaveBeach, 
    FrostbiteCaves, LostCity, NeonMixtapeTour,
    JurassicMarsh, ModernDay, ThrowbackToThePresent,
    Tutorial
}

public enum PlantQualityName
{
    Common, Rare, Epic, Legendary, Botanic
}

public enum GardenItemType
{
    Water, Compost, Fertilizer, Music, None
}

public class PlantsManager : MonoBehaviour
{
    public static PlantsManager instance;

    public class OnTickEventArgs : EventArgs { public int tick; }

    public static event EventHandler<OnTickEventArgs> OnTick;

    [Header ("Plant Presets")]
    [SerializeField] private PlantProcessAsset[] plantProcessDatas;

    [SerializeField] private GameObject sprout;
    [SerializeField] private Plant debugPlant;
    [SerializeField] private FlowerPot debugFlowerPot;
    public WorldBanks[] worldBanks;
    public WorldChanges[] worldChanges;

    [Header ("Debug")]
    [SerializeField] private int holderIdx;
    [SerializeField] private Plant plantToPut;
    [SerializeField] private Plant generatedPlant;
    [SerializeField] private FlowerPot flowerPotToPut;
    [SerializeField] private FlowerPot secondFp; 
    [SerializeField] private FlowerPot generatedPot;
    [SerializeField] private GameWorlds debugWorld;
    private const float TICK_TIMER_MAX = 1f;
    [SerializeField] private int maxTickThreshold;
    [SerializeField] private int tick;
    private float tickTimer;

    void Awake()
    {
        tick = 0;

        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    IEnumerator Testing()
    {
        yield return new WaitForSeconds(3.5f);

        generatedPlant = Instantiate(plantToPut.gameObject, transform.position, Quaternion.identity).GetComponent<Plant>();

        generatedPot.PlantPlant(generatedPlant);
    }

    void Update()
    {
        TickSystemBehaviour();
    }

    void TickSystemBehaviour()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= TICK_TIMER_MAX)
        {
            tickTimer -= TICK_TIMER_MAX;
            tick++;

            if (OnTick != null) OnTick(this, new OnTickEventArgs{ tick = tick });

            if (tick > maxTickThreshold)
                tick = 0;
        }
    }

    public int GetCurrentWorldMoney(GameWorlds world)
    {
        for (int i = 0; i < worldBanks.Length; i++)
        {
            if (worldBanks[i].world == world)
            {
                return worldBanks[i].moneyBank;
            }
        }

        return -1;
    }

    public void AddMoneyToWorldBank(GameWorlds world, int amount)
    {
        for (int i = 0; i < worldBanks.Length; i++)
        {
            if (worldBanks[i].world == world)
            {   
                if (worldBanks[i].moneyBank < 99999999)
                    worldBanks[i].moneyBank += amount;
    
                break;
            }
        }
    }

    public void AddPlantWorldChange(GameWorlds world, string plant, bool state, bool add)
    {   
        if (MusicManager.instance.GetCurrentMusic().world != world)
        {
            for (int i = 0; i < worldChanges.Length; i++)
            {
                if (worldChanges[i].world == world)
                {
                    if (state)
                    {   
                        if (add)
                        {
                            if (worldChanges[i].GetPlantOnChanges(plant, state) == -1)
                                worldChanges[i].tiredPlants.Add(new PlantChangeProfile(plant, 1));

                            else
                            {
                                int plantIndex = worldChanges[i].GetPlantOnChanges(plant, state);
                                worldChanges[i].tiredPlants[plantIndex].amount++;
                            }
                        }

                        else
                        {
                            if (worldChanges[i].GetPlantOnChanges(plant, state) != -1)
                            {
                                int plantIndex = worldChanges[i].GetPlantOnChanges(plant, state);

                                if (worldChanges[i].tiredPlants[plantIndex].amount >= 1)
                                    worldChanges[i].tiredPlants[plantIndex].amount--;
                                
                                else
                                    worldChanges[i].tiredPlants.RemoveAt(plantIndex);
                            }
                        }
                    }

                    else
                    {
                        if (add)
                        {
                            if (worldChanges[i].GetPlantOnChanges(plant, state) == -1)
                                worldChanges[i].witheredPlants.Add(new PlantChangeProfile(plant, 1));

                            else
                            {
                                int plantIndex = worldChanges[i].GetPlantOnChanges(plant, state);
                                worldChanges[i].witheredPlants[plantIndex].amount++;
                            }
                        }

                        else
                        {
                            if (worldChanges[i].GetPlantOnChanges(plant, state) != -1)
                            {
                                int plantIndex = worldChanges[i].GetPlantOnChanges(plant, state);

                                if (worldChanges[i].witheredPlants[plantIndex].amount >= 1)
                                    worldChanges[i].witheredPlants[plantIndex].amount--;
                                
                                else
                                    worldChanges[i].witheredPlants.RemoveAt(plantIndex);
                            }
                        }
                    }

                    break;
                }
            }
        }
    }

    public void AddMoneyWorldChange(int amount)
    {

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

[System.Serializable]
public class WorldBanks
{
    public GameWorlds world;
    public int moneyBank;
}

[System.Serializable]
public class WorldChanges
{
    public GameWorlds world;
    public List<PlantChangeProfile> tiredPlants, witheredPlants = new List<PlantChangeProfile>();
    public int producedMoneyUntilPoint;
    
    public int GetPlantOnChanges(string _plant, bool tired)
    {   
        if (tired)
        {
            for (int i = 0; i < tiredPlants.Count; i++)
            {
                if (tiredPlants[i].plantName == _plant)
                    return i;
            }
        }

        else
        {
            for (int i = 0; i < witheredPlants.Count; i++)
            {
                if (witheredPlants[i].plantName == _plant)
                    return i;
            }
        }

        return -1;
    }
}

[System.Serializable]
public class PlantChangeProfile
{
    public string plantName;
    public int amount;

    public PlantChangeProfile(string plantName, int amount)
    {
        this.plantName = plantName;
        this.amount = amount;
    }
}
