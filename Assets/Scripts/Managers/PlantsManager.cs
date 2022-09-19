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
    public PlantAsset[] allPlantAssets;
    public WorldBanks[] worldBanks;
    public QualityStandards common, rare, epic, legend, botanic;
    public List<WorldChanges> worldChanges = new List<WorldChanges>();

    private const float TICK_TIMER_MAX = 1f;
    [SerializeField] private int maxTickThreshold;
    private int tick;
    private float tickTimer;

    void Awake()
    {
        tick = 0;

        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        AutoSetPrices();
    }

    void OnApplicationQuit()
    {
        RemoveStats();
    }

    public float GetRandomMultiplier(Vector2 range)
    {
        return UnityEngine.Random.Range(range.x, range.y);
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

    public void SetWorldMoney(GameWorlds world, int value)
    {
        for (int i = 0; i < worldBanks.Length; i++)
        {
            if (worldBanks[i].world == world)
            {
                worldBanks[i].moneyBank = value;
                break;
            }
        }
    }

    public void AddMoneyToWorldBank(GameWorlds world, int amount)
    {
        for (int i = 0; i < worldBanks.Length; i++)
        {
            if (worldBanks[i].world == world)
            {   
                if (worldBanks[i].moneyBank < 50000)
                {
                    worldBanks[i].moneyBank += amount;

                    if (worldBanks[i].moneyBank > 50000) worldBanks[i].moneyBank = 50000;

                    UIManager.instance.SetBankMoneyText(GetCurrentWorldMoney(world));
                }
    
                break;
            }
        }
    }

    public void AddPlantWorldChange(GameWorlds world, string plant, bool state, bool add)
    {   
        if (MusicManager.instance.GetCurrentMusic().world != world)
        {
            for (int i = 0; i < worldChanges.Count; i++)
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

                    DataCollector.instance.SetWorldChanges(world, worldChanges[i]);

                    break;
                }
            }
        }
    }

    public void ClearWorldChanges(GameWorlds world)
    {
        for (int i = 0; i < worldChanges.Count; i++)
        {
            worldBanks[i].moneyBank += worldChanges[i].producedMoneyUntilPoint;
            DataCollector.instance.SetWorldBankMoney(world, worldBanks[i].moneyBank);

            worldChanges[i].witheredPlants.Clear();
            worldChanges[i].tiredPlants.Clear();
            worldChanges[i].producedMoneyUntilPoint = 0;
        }
    }

    public void AddMoneyWorldChange(GameWorlds worlds, int amount)
    {
        for (int i = 0; i < worldChanges.Count; i++)
        {
            if (worldChanges[i].world == worlds && worldChanges[i].producedMoneyUntilPoint < 50000)
            {
                worldChanges[i].producedMoneyUntilPoint += amount;

                if (worldChanges[i].producedMoneyUntilPoint > 50000) worldChanges[i].producedMoneyUntilPoint = 50000;

                DataCollector.instance.SetWorldChanges(worlds, worldChanges[i]);
                break;
            }
        }
    }

    public bool ChangesInWorld(GameWorlds world)
    {
        for (int i = 0; i < worldChanges.Count; i++)
        {
            if (worldChanges[i].world == world)
                return (worldChanges[i].tiredPlants.Count > 0 || worldChanges[i].witheredPlants.Count > 0 || worldChanges[i].producedMoneyUntilPoint > 0);
        }

        return false;
    }

    public GameObject GetSprout()
    {
        return sprout;
    }

    void AutoSetPrices()
    {
        for (int i = 0; i < allPlantAssets.Length; i++)
        {
            PlantAsset pa = allPlantAssets[i];
            QualityStandards qs = null;

            switch (pa.plantQuality.quality)
            {
                case PlantQualityName.Common:
                    qs = common;
                break;

                case PlantQualityName.Rare:
                    qs = rare;
                break;

                case PlantQualityName.Epic:
                    qs = epic;
                break;

                case PlantQualityName.Legendary:
                    qs = legend;
                break;

                case PlantQualityName.Botanic:
                    qs = botanic;
                break;
            }

            if (!pa.autoSettedPrice)
            {
                pa.plantLevels[0].producingTime = GetRounded(qs.plantProductionTime * GetWorldMultiplier(pa.appearsIn) * GetRandomMultiplier(GetWorldRange(pa.appearsIn)));
                pa.plantLevels[0].producedCoins = GetRounded(qs.producedCoins * GetWorldMultiplier(pa.appearsIn) * GetRandomMultiplier(GetWorldRange(pa.appearsIn)));
                pa.plantLevels[0].energyLevel = GetRounded(qs.plantEnergy * GetWorldMultiplier(pa.appearsIn) * GetRandomMultiplier(GetWorldRange(pa.appearsIn)));
                pa.plantLevels[0].plantLife = GetRounded(qs.plantLife * GetWorldMultiplier(pa.appearsIn) * GetRandomMultiplier(GetWorldRange(pa.appearsIn)));

                pa.plantLevels[1].producingTime = GetRounded(pa.plantLevels[0].producingTime * 1.5f);
                pa.plantLevels[1].producedCoins = GetRounded(pa.plantLevels[0].producedCoins * 1.5f);
                pa.plantLevels[1].energyLevel = GetRounded(pa.plantLevels[0].energyLevel * 1.5f);
                pa.plantLevels[1].plantLife = GetRounded(pa.plantLevels[0].plantLife * 1.5f);

                pa.plantLevels[2].producingTime = GetRounded(pa.plantLevels[0].producingTime * 2.2f);
                pa.plantLevels[2].producedCoins = GetRounded(pa.plantLevels[0].producedCoins * 2.2f);
                pa.plantLevels[2].energyLevel = GetRounded(pa.plantLevels[0].energyLevel * 2.2f);
                pa.plantLevels[2].plantLife = GetRounded(pa.plantLevels[0].plantLife * 2.2f);

                int expProductionZe = pa.plantLevels[0].producingTime * pa.plantLevels[0].producedCoins * pa.plantLevels[0].energyLevel * pa.plantLevels[0].plantLife;
                pa.unlockPrice = pa.buyPrice = pa.plantLevels[0].sellPrice = GetRounded(expProductionZe * 0.02f);

                int expProductionTw = pa.plantLevels[1].producingTime * pa.plantLevels[1].producedCoins * pa.plantLevels[1].energyLevel * pa.plantLevels[1].plantLife;
                pa.plantLevels[1].upgradePrice = GetRounded(expProductionTw * 0.02f);
                pa.plantLevels[1].sellPrice = GetRounded((expProductionTw * 0.02f) * 1.5f);

                int expProductionTh = pa.plantLevels[2].producingTime * pa.plantLevels[2].producedCoins * pa.plantLevels[2].energyLevel * pa.plantLevels[2].plantLife;
                pa.plantLevels[2].upgradePrice = GetRounded(expProductionTh * 0.02f);
                pa.plantLevels[2].sellPrice = GetRounded((expProductionTh * 0.02f) * 1.5f);
            }
        }
    }

    void RemoveStats()
    {
        for (int i = 0; i < allPlantAssets.Length; i++)
        {
            PlantAsset pa = allPlantAssets[i];
            QualityStandards qs = null;

            switch (pa.plantQuality.quality)
            {
                case PlantQualityName.Common:
                    qs = common;
                break;

                case PlantQualityName.Rare:
                    qs = rare;
                break;

                case PlantQualityName.Epic:
                    qs = epic;
                break;

                case PlantQualityName.Legendary:
                    qs = legend;
                break;

                case PlantQualityName.Botanic:
                    qs = botanic;
                break;
            }

            if (!pa.autoSettedPrice)
            {
                pa.plantLevels[0].producingTime = 0;
                pa.plantLevels[0].producedCoins = 0;
                pa.plantLevels[0].energyLevel = 0;
                pa.plantLevels[0].plantLife = 0;

                pa.plantLevels[1].producingTime = 0;
                pa.plantLevels[1].producedCoins = 0;
                pa.plantLevels[1].energyLevel = 0;
                pa.plantLevels[1].plantLife = 0;

                pa.plantLevels[2].producingTime = 0;
                pa.plantLevels[2].producedCoins = 0;
                pa.plantLevels[2].energyLevel = 0;
                pa.plantLevels[2].plantLife = 0;

                pa.unlockPrice = 0;

                pa.plantLevels[1].upgradePrice = 0;
                pa.plantLevels[1].sellPrice = 0;

                pa.plantLevels[2].upgradePrice = 0;
                pa.plantLevels[2].sellPrice = 0;

                pa.autoSettedPrice = true;
            }
        }
    }

    public int GetRounded(float val)
    {
        return (int)(Math.Round(val));
    }

    public float GetWorldMultiplier(GameWorlds world)
    {
        switch (world)
        {
            case GameWorlds.Tutorial: return 1f;
            case GameWorlds.JurassicMarsh: return 1.05f;
            case GameWorlds.NeonMixtapeTour: return 1.1f;
            case GameWorlds.DarkAges: return 1.15f;
            case GameWorlds.PirateSeas: return 1.2f;
            case GameWorlds.FarFuture: return 1.25f;
            case GameWorlds.LostCity: return 1.3f;
            case GameWorlds.WildWest: return 1.35f;
            case GameWorlds.BigWaveBeach: return 1.4f;
            case GameWorlds.FrostbiteCaves: return 1.45f;
            case GameWorlds.AncientEgypt: return 1.5f;
            case GameWorlds.ModernDay: return 1.55f;
        }
        
        return -1f;
    }

    public Vector2 GetWorldRange(GameWorlds world)
    {
        switch (world)
        {
            case GameWorlds.Tutorial: return new Vector2(1f, 1f);
            case GameWorlds.JurassicMarsh: return new Vector2(1f, 1.05f);
            case GameWorlds.NeonMixtapeTour: return new Vector2(1.06f, 1.1f);
            case GameWorlds.DarkAges: return new Vector2(1.11f, 1.15f);
            case GameWorlds.PirateSeas: return new Vector2(1.16f, 1.2f);
            case GameWorlds.FarFuture: return new Vector2(1.21f, 1.25f);
            case GameWorlds.LostCity: return new Vector2(1.26f, 1.3f);
            case GameWorlds.WildWest: return new Vector2(1.31f, 1.35f);
            case GameWorlds.BigWaveBeach: return new Vector2(1.36f, 1.4f);
            case GameWorlds.FrostbiteCaves: return new Vector2(1.41f, 1.45f);
            case GameWorlds.AncientEgypt: return new Vector2(1.46f, 1.5f);
            case GameWorlds.ModernDay: return new Vector2(1.51f, 1.55f);
        }

        return Vector2.zero;
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

[System.Serializable]
public class QualityStandards
{
    public float plantProductionTime;
    public int producedCoins;
    public float plantEnergy;
    public float plantLife;
}
