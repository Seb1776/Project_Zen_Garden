using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class SerializableData
{
    public List<GardenTable> savedTables = new List<GardenTable>();
    public List<SeedDataContainer> savedSeeds = new List<SeedDataContainer>();
    public List<FlowerPotDataContainer> flowerPotsDatas = new List<FlowerPotDataContainer>();
    public List<GameWorlds> unlockedWorlds = new List<GameWorlds>();
    public int playerWaters, playerComposts, playerFertilizer, playerPhonographs;
    public string gardenName, playerLastWorld;
    public int playerCoins;
    public bool isOnTutorial;
    public float spentTime;

    public SerializableData (List<GardenTable> savedTables, List<SeedDataContainer> savedSeeds, List<FlowerPotDataContainer> flowerPotDatas,
        List<GameWorlds> unlockedWorlds, int playerWaters, int playerComposts, int playerFertilizer, int playerPhonographs, string gardenName, string playerLastWorld, bool isOnTutorial,
        float spentTime, int playerCoins)
    {
        this.savedTables = savedTables;
        this.savedSeeds = savedSeeds;
        this.flowerPotsDatas = flowerPotDatas;
        this.unlockedWorlds = unlockedWorlds;
        this.playerWaters = playerWaters; this.playerComposts = playerComposts; this.playerFertilizer = playerFertilizer;
        this.playerPhonographs = playerPhonographs;
        this.gardenName = gardenName;
        this.playerLastWorld = playerLastWorld;
        this.isOnTutorial = isOnTutorial;
        this.spentTime = spentTime;
        this.playerCoins = playerCoins;
    }
}

[System.Serializable]
public class WorldHolders
{
    public GameWorlds world;
    public FlowerPotHolder[] holders;
}

[System.Serializable]
public class SeedDataContainer
{
    public string plantAssetName;
    public string plantWorld;
    public int plantAmount;

    public SeedDataContainer(string plantAssetName, string plantWorld, int plantAmount)
    {
        this.plantAssetName = plantAssetName;
        this.plantWorld = plantWorld;
        this.plantAmount = plantAmount;
    }
}

[System.Serializable]
public class FlowerPotDataContainer
{
    public FlowerPotType flowerPotType;
    public int amount;
}

public class DataCollector : MonoBehaviour
{
    public static DataCollector instance;
    [SerializeField] private float autoSaveTime;
    [NonReorderable]
    public List<GardenTable> worldTables = new List<GardenTable>();
    public List<SeedDataContainer> seeds = new List<SeedDataContainer>();
    public List<FlowerPotDataContainer> flowerPotsDatas = new List<FlowerPotDataContainer>();
    public List<WorldHolders> worldHolders = new List<WorldHolders>();
    public List<GameWorlds> unlockedWorlds = new List<GameWorlds>();
    public int playerCoins;
    public string gameFileLetter;
    public int playerWaters, playerComposts, playerFertilizer, playerPhonographs;
    public string gardenName, playerLastWorld;
    public bool isOnTutorial;
    public float spentTime;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        gameFileLetter = GameObject.FindGameObjectWithTag("MainMenu").GetComponent<MainMenu>().loadedLetter;
        LoadData();
    }

    public void SaveData()
    {
        playerCoins = Player.instance.GetPlayerMoney();
        SeedDatabase.instance.SendGardenDataToCollector();
        spentTime = GameManager.instance.GetSpentTime();
        SerializableData zenData = new SerializableData(worldTables, seeds, flowerPotsDatas, unlockedWorlds, playerWaters, playerComposts, playerFertilizer, playerPhonographs, 
            gardenName, playerLastWorld, isOnTutorial, spentTime, playerCoins);

        using (StreamWriter stream = new StreamWriter(Application.persistentDataPath + "/ZenGardenVR_" + gameFileLetter + ".json"))
        {
            string json = JsonUtility.ToJson(zenData);
            stream.Write(json);
        }
    }

    public void LoadData()
    {
        MainMenu mm = GameObject.FindGameObjectWithTag("MainMenu").GetComponent<MainMenu>();

        if (File.Exists(Application.persistentDataPath + "/ZenGardenVR_" + gameFileLetter + ".json"))
        {
            using (StreamReader reader = new StreamReader(Application.persistentDataPath + "/ZenGardenVR_" + gameFileLetter + ".json"))
            {
                string json = reader.ReadToEnd();
                SerializableData loadedData = JsonUtility.FromJson<SerializableData>(json);
                worldTables = loadedData.savedTables;
                seeds = loadedData.savedSeeds;
                flowerPotsDatas = loadedData.flowerPotsDatas;
                unlockedWorlds = loadedData.unlockedWorlds;
                GameManager.instance.spentTime = loadedData.spentTime;
                MusicAsset ma = Resources.Load<MusicAsset>("Music/Datas/" + loadedData.playerLastWorld);
                MusicManager.instance.ChangeWithoutTransition(ma);
                SetTutorialState(loadedData.isOnTutorial);

                RecreateData(loadedData);
            }
        }

        else
        {
            SetTutorialState(true);
            playerLastWorld = "Tutorial";
            GameManager.instance.FirstTimeTutorial();
            SeedDatabase.instance.SendGardenDataToCollector();
            UIManager.instance.CheckAvailabilityForPinatas();
            //MusicManager.instance.ChangeWithoutTransition(Resources.Load<MusicAsset>("Music/Datas/Tutorial"));

            SetGardenName(mm.newSetGardenName);
            mm.newSetGardenName = string.Empty;

            StartCoroutine(AutoSave());
        }

        mm.transitionBall.SetTrigger("detransition");
        GameManager.instance.startCounting = true;
        StartCoroutine(WaitToDeleteMain(7f, mm.gameObject, mm.transitionBall.gameObject));
    }

    IEnumerator WaitToDeleteMain(float delay, GameObject _a, GameObject _b)
    {
        Destroy(_a.gameObject);
        yield return new WaitForSeconds(delay);
        Destroy(_b.gameObject);
    }

    public void AddUnlockedWorld(GameWorlds worlds)
    {
        unlockedWorlds.Add(worlds);
    }

    void RecreateData(SerializableData sd)
    {
        for (int i = 0; i < worldTables.Count; i++)
        {
            for (int j = 0; j < worldTables[i].flowerPotsOnTable.Count; j++)
            {
                FlowerPot fpPref = Resources.Load<FlowerPot>("Prefabs/FlowerPots/" + worldTables[i].flowerPotsOnTable[j].flowerPotInSpace);
                FlowerPot createdFlowerPot = SetSpecificFlowerPot(worldTables[i].GetFlowerPotHolder(worldTables[i].flowerPotsOnTable[j].holderIdx), fpPref);
                createdFlowerPot.createdIn = worldTables[i].correspondingWorld.ToString();

                if (worldTables[i].flowerPotsOnTable[j].plantInSpace.plantName != string.Empty)
                {
                    PlantAsset pa = Resources.Load<PlantAsset>("PlantsAssets/" + worldTables[i].flowerPotsOnTable[j].plantInSpace.plantWorld + "/" + worldTables[i].flowerPotsOnTable[j].plantInSpace.plantName);
                    SetSpecificPlant(createdFlowerPot, pa.uiPlant.gameObject, worldTables[i].flowerPotsOnTable[j].plantInSpace);
                }
            }
        }

        for (int i = 0; i < seeds.Count; i++)
            if (seeds[i].plantAssetName != "Peashooter")
                SeedDatabase.instance.SetPlantAssetFromData(seeds[i]);

        for (int i = 0; i < flowerPotsDatas.Count; i++)
            SeedDatabase.instance.SetFlowerPotsFromLoad(flowerPotsDatas[i]);
        
        for (int i = 0; i < unlockedWorlds.Count; i++)
        {   
            MusicAsset ma = Resources.Load<MusicAsset>("Music/Datas/" + unlockedWorlds[i].ToString());

            if (ma.world == GameWorlds.JurassicMarsh)
                SeedDatabase.instance.jurassicBlock.SetActive(false);

            MusicManager.instance.DeactivateWorldLock(ma);
        }

        for (int i = 0; i < unlockedWorlds.Count; i++)
        {
            MusicAsset ma = Resources.Load<MusicAsset>("Music/Datas/" + unlockedWorlds[i].ToString());
            MusicManager.instance.CheckNextWorldPanelToOpen(ma);
        }

        SeedDatabase.instance.SetGardenDataFromCollector(sd.playerWaters, sd.playerComposts, sd.playerFertilizer, sd.playerPhonographs);

        foreach (GardenItem gi in SeedDatabase.instance.waterUI.items)
            gi.CheckForUsability();

        foreach (GardenItem gi in SeedDatabase.instance.compostUI.items)
            gi.CheckForUsability();
        
        foreach (GardenItem gi in SeedDatabase.instance.fertilizerUI.items)
            gi.CheckForUsability();
        
        foreach (GardenItem gi in SeedDatabase.instance.phonographUI.items)
            gi.CheckForUsability();
        
        if (!sd.isOnTutorial)
            MusicManager.instance.RemoveTutorialStuff();
        
        else
        {
            if (SeedDatabase.instance.GetTotalUnlockedPlants() >= 5)
                SeedDatabase.instance.jurassicBlock.SetActive(false);
        }

        Player.instance.SetMoneyAmount(sd.playerCoins);
        UIManager.instance.CheckAvailabilityForPinatas();
        playerLastWorld = sd.playerLastWorld;
        gardenName = sd.gardenName;

        StartCoroutine(AutoSave());
    }

    public void SetPlayerCoins(int playerMoney)
    {
        playerCoins = playerMoney;
    }

    public void SetTutorialState(bool onTutorial)
    {
        isOnTutorial = onTutorial;
        GameManager.instance.onTutorial = onTutorial;
    }

    public void SetGardenName(string name)
    {
        gardenName = name;
    }

    public void SetLastVisitedWorld(GameWorlds world)
    {
        playerLastWorld = world.ToString();
    }

    public void UpdateFlowerPotData(FlowerPotType fpt, int amount)
    {
        for (int i = 0; i < flowerPotsDatas.Count; i++)
            if (fpt == flowerPotsDatas[i].flowerPotType)
                flowerPotsDatas[i].amount = amount;
    }

    public void AddNewSeedPacket(string plant, string world, int amount)
    {   
        if (plant != "Peashooter")
            seeds.Add(new SeedDataContainer(plant, world, amount));
    }

    public void UpdateSeedPacket(string plant, int amount)
    {
        if (GetSeedPacket(plant) != null)
        {
            SeedDataContainer sdc = GetSeedPacket(plant);
            sdc.plantAmount = amount;
        }
    }

    public SeedDataContainer GetSeedPacket(string plant)
    {
        for (int i = 0; i < seeds.Count; i++)
            if (seeds[i].plantAssetName == plant)
                return seeds[i];

        return null;
    }

    public FlowerPot SetSpecificFlowerPot(FlowerPotHolder holder, FlowerPot flowerPot)
    {
        FlowerPot fp = Instantiate(flowerPot, transform.position, Quaternion.identity);

        fp.transform.position = fp.startPos = holder.transform.position;
        fp.transform.rotation = holder.transform.rotation;
        holder.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        holder.GetComponent<Collider>().enabled = false;
        fp.inPositionOfHolder = holder;
        fp.transform.parent = holder.transform;
        fp.createdIn = MusicManager.instance.currentWorld.ToString();
        fp.setted = true;
        fp.triggerColl.enabled = true;

        return fp;
    }

    public void SetPlantRevenueMult(GameWorlds world, FlowerPot flowerPot, float mult)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot fps = _gt.GetFlowerPotSpot(flowerPot.inPositionOfHolder.holderIdx);

            if (fps != null) fps.SetPlantRevenueMult(mult);
        }
    }

    public void UdpateFlowerPotHolder(GameWorlds world, FlowerPotHolder previousFp, FlowerPotHolder newFp)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot fps = _gt.GetFlowerPotSpot(previousFp.holderIdx);
            fps.holderIdx = newFp.holderIdx;
        }
    }

    public void SetPlantGrowThreshold(GameWorlds world, FlowerPot flowerPot, int grow)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot fps = _gt.GetFlowerPotSpot(flowerPot.inPositionOfHolder.holderIdx);

            if (fps != null) fps.SetPlantGrowThreshold(grow);
        }
    }

    public void SetSpecificPlant(FlowerPot flowerPot, GameObject plant, PlantSpot plantSpot)
    {
        flowerPot.PlantPlantFromLoad(plant, plantSpot);
    }

    public void AddFlowerPot(GameWorlds world, FlowerPot flowerPot)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            _gt.AddFlowerPot(flowerPot.flowerPotAsset.flowerPotName, flowerPot.inPositionOfHolder.holderIdx, flowerPot.createdIn);
        }
    }

    public void RemoveFlowerPot(GameWorlds world, FlowerPot flowerPot)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            _gt.RemoveFlowerPot(flowerPot.inPositionOfHolder.holderIdx);
        }
    }

    public void AddPlant(GameWorlds world, FlowerPot flowerPot, Plant plant)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot fps = _gt.GetFlowerPotSpot(flowerPot.inPositionOfHolder.holderIdx);

            if (fps != null) fps.AddPlant(plant);
        }
    }

    public void RemovePlant(GameWorlds world, FlowerPot flowerPot)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot fps = _gt.GetFlowerPotSpot(flowerPot.inPositionOfHolder.holderIdx);

            if (fps != null) fps.RemovePlant();
        }
    }

    public void SetPlantGardenState(GameWorlds world, string gardening, FlowerPot flowerPot)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot fps = _gt.GetFlowerPotSpot(flowerPot.inPositionOfHolder.holderIdx);

            if (fps != null) fps.SetPlantGardeningState(gardening);
        }
    }

    public void ReplacePlantInFlowerPot(GameWorlds world, Plant plant, FlowerPot oldFp, FlowerPot newFp)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot oldFps = _gt.GetFlowerPotSpot(oldFp.inPositionOfHolder.holderIdx);
            FlowerPotSpot newFps = _gt.GetFlowerPotSpot(newFp.inPositionOfHolder.holderIdx);

            oldFps.RemovePlant();
            newFps.AddPlant(plant);
        }
    }

    public void SetPlantGardenData(GameWorlds world, FlowerPot flowerPot, string dataType, int current)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot fps = _gt.GetFlowerPotSpot(flowerPot.inPositionOfHolder.holderIdx);

            if (fps != null) fps.SetPlantGardenData(dataType, current);
        }
    }

    public void SetPlantFullGrownData(GameWorlds world, FlowerPot flowerPot, bool grown, bool ultraGrown)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            FlowerPotSpot fps = _gt.GetFlowerPotSpot(flowerPot.inPositionOfHolder.holderIdx);

            if (fps != null) fps.SetPlantFullGrownData(grown, ultraGrown);
        }
    }

    public GardenTable GetTableFromWorld(GameWorlds _world)
    {
        for (int i = 0; i < worldTables.Count; i++)
            if (_world == worldTables[i].correspondingWorld)
                return worldTables[i];

        return null;
    }

    IEnumerator AutoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveTime);
            SaveData();
        }
    }
}

[System.Serializable]
public class GardenTable
{
    public GameWorlds correspondingWorld;
    [NonReorderable]
    public List<FlowerPotSpot> flowerPotsOnTable = new List<FlowerPotSpot>();

    public void AddFlowerPot(string flowerPot, int holderIdx, string createdIn)
    {
        flowerPotsOnTable.Add(new FlowerPotSpot(flowerPot, holderIdx, createdIn));
    }

    public void RemoveFlowerPot(int flowerPotIdx)
    {
        for (int i = 0; i < flowerPotsOnTable.Count; i++)
        {
            if (flowerPotsOnTable[i].holderIdx == flowerPotIdx)
            {
                FlowerPotSpot fps = flowerPotsOnTable[i];
                flowerPotsOnTable.Remove(fps);
            }
        }
    }

    public FlowerPotHolder GetFlowerPotHolder(int idx)
    {
        for (int i = 0; i < DataCollector.instance.worldHolders.Count; i++)
        {
            if (DataCollector.instance.worldHolders[i].world == correspondingWorld)
            {
                for (int j = 0; j < DataCollector.instance.worldHolders[i].holders.Length; j++)
                    if (DataCollector.instance.worldHolders[i].holders[j].holderIdx == idx)
                        return DataCollector.instance.worldHolders[i].holders[j];
            }
        }

        Debug.LogError("Couldn't find Flower Pot Holder with Index: " + idx);
        return null;
    }

    public FlowerPotSpot GetFlowerPotSpot(int _holderIdx)
    {
        for (int i = 0; i < flowerPotsOnTable.Count; i++)
        {
            if (flowerPotsOnTable[i].holderIdx == _holderIdx)
                return flowerPotsOnTable[i];
        }

        Debug.LogError("Couldn't find Flower Pot Spot with Holder Index: " + _holderIdx);
        return null;
    }
}

[System.Serializable]
public class FlowerPotSpot
{
    public string flowerPotInSpace = string.Empty;
    public string fpCreatedIn = string.Empty;
    public int holderIdx;
    public PlantSpot plantInSpace = new PlantSpot();

    public FlowerPotSpot (string flowerPotInSpace, int holderIdx, string fpCreatedIn)
    {
        this.flowerPotInSpace = flowerPotInSpace;
        this.holderIdx = holderIdx;
        this.fpCreatedIn = fpCreatedIn;
    }

    public void AddPlant(Plant plant)
    {
        PlantSpot _plantInSpace = new PlantSpot();

        _plantInSpace.plantName = plant.plantData.name;
        _plantInSpace.plantWorld = plant.plantData.appearsIn.ToString();
        _plantInSpace.plantGrowThreshold = plant.currentGrowThreshold;
        _plantInSpace.plantMultRevenue = plant.revenueMultiplier;
        _plantInSpace.waterTotalUses = plant.waterRange.y; plantInSpace.waterCurrentUses = plant.waterRange.x;
        _plantInSpace.compostTotalUses = plant.compostRange.y; plantInSpace.compostCurrentUses = plant.compostRange.x;
        _plantInSpace.fertilizerTotalUses = plant.fertilizerRange.y; plantInSpace.fertilizerCurrentUses = plant.fertilizerRange.x;
        _plantInSpace.phonographTotalUses = plant.musicRange.y; plantInSpace.phonographCurrentUses = plant.musicRange.x;
        _plantInSpace.plantIsGrown = plant.fullyGrown;

        plantInSpace = _plantInSpace;
    }

    public void SetPlantGardeningState(string state)
    {
        plantInSpace.plantLastRequire = state;
    }

    public void SetPlantFullGrownData(bool isGrown, bool superGrown)
    {
        plantInSpace.plantIsGrown = isGrown;
        plantInSpace.plantIsFullyGrown = superGrown;
    }

    public void SetPlantGardenData(string dataType, int current)
    {
        switch (dataType)
        {
            case "Water":
                plantInSpace.waterCurrentUses = current;
            break;

            case "Compost":
                plantInSpace.compostCurrentUses = current;
            break;

            case "Fertilizer":
                plantInSpace.fertilizerCurrentUses = current;
            break;

            case "Music":
                plantInSpace.phonographCurrentUses = current;
            break;
        }
    }

    public void RemovePlant()
    {
        BlankPlantSpace();
    }

    public void SetPlantGrowThreshold(int grow)
    {
        plantInSpace.plantGrowThreshold = grow;
    }

    public void SetPlantRevenueMult(float mult)
    {
        plantInSpace.plantMultRevenue = mult;
    }

    void BlankPlantSpace()
    {
        plantInSpace.plantName = string.Empty;
        plantInSpace.plantWorld = string.Empty;
        plantInSpace.plantLastRequire = string.Empty;
        plantInSpace.plantGrowThreshold = 0; plantInSpace.plantMultRevenue = 0f;
        plantInSpace.waterTotalUses = plantInSpace.waterCurrentUses = 0;
        plantInSpace.compostTotalUses = plantInSpace.compostCurrentUses = 0;
        plantInSpace.fertilizerTotalUses = plantInSpace.fertilizerCurrentUses = 0;
        plantInSpace.phonographTotalUses = plantInSpace.phonographCurrentUses = 0;
    }
}

[System.Serializable]
public class PlantSpot
{
    public string plantName = string.Empty;
    public string plantWorld = string.Empty;
    public string plantLastRequire = string.Empty;
    public int plantGrowThreshold;
    public float plantMultRevenue;
    public int waterTotalUses, compostTotalUses, fertilizerTotalUses, phonographTotalUses;
    public int waterCurrentUses, compostCurrentUses, fertilizerCurrentUses, phonographCurrentUses;
    public bool plantIsGrown, plantIsFullyGrown;
}
