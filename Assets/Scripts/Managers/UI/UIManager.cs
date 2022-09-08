using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private Player player;
    [SerializeField] private SeedDatabase seedDatabase;
    [SerializeField] private GameObject pinata;
    [SerializeField] private GameObject uiCover;
    [SerializeField] private GameObject[] plantSections;
    public Transform pinataGridPanel;
    public GameObject pinataRewardPanel;
    [Header("UI")]
    [SerializeField] private Text coins;
    [SerializeField] private GameObject wywPanel;
    [SerializeField] private Text wywCoins;
    [SerializeField] private GameObject tiredNone, witheredNone;
    [SerializeField] private Transform tiredPanel, witheredPanel;
    private Pinata creaPinata;
    [SerializeField] private string activePlants;
    [Header("Almanac UI")]
    [SerializeField] private Text plantNameT;
    [SerializeField] private Text plantBuyPriceT, plantSellPriceT, plantExtraPercT, plantQualityT, plantFlowerPotsT, plantDescT;
    [SerializeField] private Image plantBackGround;
    [SerializeField] private Transform plantSpawn;
    [Header("Pinatas UI")]
    public Transform pinataRewardsSpawn;
    [SerializeField] private GameObject pinataMenu;
    [SerializeField] private Image pinataImage;
    [SerializeField] private GameObject pinataContinueButton;
    [SerializeField][NonReorderable] private PinataRewardsPlaceholders[] pinataQualitiesChances;
    [SerializeField] private Button buyPinataButton;
    [SerializeField] private GameObject buyPinataCoin;
    [SerializeField] private PinataAsset[] allPinatas;
    [SerializeField] private Image menuPinataImage;
    [SerializeField] private Text pinataSizeT;
    [SerializeField] private Text pinataNameT;
    [SerializeField] private Text pinataPriceT;
    [SerializeField][NonReorderable] private PinatasUIQuality[] pinatasUI;
    [SerializeField] private List<RewardedPlants> rewComm, rewRar, rewEpc, rewLeg = new List<RewardedPlants>();
    private int currentPinatasIndex, currentSizesIndex;
    public Text pinataRewardText;
    private Plant spawnedUIPlant;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        InitPinatasMenu();
    }

    void Update()
    {
        PlayerUI();
    }

    void PlayerUI()
    {
        coins.text = "$ " + player.GetPlayerMoney();
    }

    public void SpawnFlowerPotOnHand(FlowerPot flowerPot)
    {
        if (Player.instance.holdingPlant == null && seedDatabase.CanUseFlowerPot(flowerPot.flowerPotAsset))
        {
            SoundEffectsManager.instance.PlaySoundEffectNC("click");

            if (player.GetHoldingFlowerPot() == null)
                CreateFlowerPotOnPlayerHand(flowerPot.gameObject);

            else
            {
                if (player.GetHoldingFlowerPot().gameObject.name != flowerPot.gameObject.name)
                {
                    player.DestroyHoldingFlowerPot();
                    CreateFlowerPotOnPlayerHand(flowerPot.gameObject);
                }
            }

            seedDatabase.TriggerHolders(true);
        }
    }

    public void BackToStartMenu()
    {
        DataCollector.instance.SaveData();
        StartCoroutine(LoadScene(0));
    }

    IEnumerator LoadScene(int index)
    {
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(index);

        while (!asyncOp.isDone)
            yield return null;
    }

    public void BuyGardenItem(string _git)
    {
        GardenItemType? git = null;

        switch (_git)
        {
            case "water":
                git = GardenItemType.Water;
                break;

            case "compost":
                git = GardenItemType.Compost;
                break;

            case "fertilizer":
                git = GardenItemType.Fertilizer;
                break;

            case "phonograph":
                git = GardenItemType.Music;
                break;
        }

        switch (git)
        {
            case GardenItemType.Water:
                if (Player.instance.CanSpendMoney(SeedDatabase.instance.waterUI.gardenPrice) &&
                    SeedDatabase.instance.GardenIsBuyable(GardenItemType.Water))
                {
                    SeedDatabase.instance.GardenUse(GardenItemType.Water, true);

                    foreach (GardenItem gi in SeedDatabase.instance.waterUI.items)
                        gi.CheckForUsability();

                    Player.instance.SpendMoney(SeedDatabase.instance.waterUI.gardenPrice);
                    SoundEffectsManager.instance.PlaySoundEffectNC("money");
                }

                else
                    SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
                break;

            case GardenItemType.Compost:
                if (Player.instance.CanSpendMoney(SeedDatabase.instance.compostUI.gardenPrice) &&
                    SeedDatabase.instance.GardenIsBuyable(GardenItemType.Compost))
                {
                    SeedDatabase.instance.GardenUse(GardenItemType.Compost, true);

                    foreach (GardenItem gi in SeedDatabase.instance.compostUI.items)
                        gi.CheckForUsability();

                    Player.instance.SpendMoney(SeedDatabase.instance.compostUI.gardenPrice);
                    SoundEffectsManager.instance.PlaySoundEffectNC("money");
                }

                else
                    SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
                break;

            case GardenItemType.Fertilizer:
                if (Player.instance.CanSpendMoney(SeedDatabase.instance.fertilizerUI.gardenPrice) &&
                    SeedDatabase.instance.GardenIsBuyable(GardenItemType.Fertilizer))
                {
                    SeedDatabase.instance.GardenUse(GardenItemType.Fertilizer, true);

                    foreach (GardenItem gi in SeedDatabase.instance.fertilizerUI.items)
                        gi.CheckForUsability();

                    Player.instance.SpendMoney(SeedDatabase.instance.fertilizerUI.gardenPrice);
                    SoundEffectsManager.instance.PlaySoundEffectNC("money");
                }

                else
                    SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
                break;

            case GardenItemType.Music:
                if (Player.instance.CanSpendMoney(SeedDatabase.instance.phonographUI.gardenPrice) &&
                    SeedDatabase.instance.GardenIsBuyable(GardenItemType.Music))
                {
                    SeedDatabase.instance.GardenUse(GardenItemType.Music, true);

                    foreach (GardenItem gi in SeedDatabase.instance.phonographUI.items)
                        gi.CheckForUsability();

                    Player.instance.SpendMoney(SeedDatabase.instance.phonographUI.gardenPrice);
                    SoundEffectsManager.instance.PlaySoundEffectNC("money");
                }

                else
                    SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
                break;
        }
    }

    public void TriggerWhileYouWhereGone(GameWorlds world)
    {
        List<WorldChanges> _wc = PlantsManager.instance.worldChanges;
        List<Animator> createds = new List<Animator>();

        for (int i = 0; i < _wc.Count; i++)
        {      
            if (_wc[i].world == world)
            {
                tiredNone.SetActive(_wc[i].tiredPlants.Count == 0);
                witheredNone.SetActive(_wc[i].witheredPlants.Count == 0);
                wywCoins.text = "$ " + _wc[i].producedMoneyUntilPoint.ToString("N0");

                for (int j = 0; j < _wc[i].tiredPlants.Count; j++)
                {
                    GameObject seedUI = Instantiate(
                        Resources.Load<GameObject>("Prefabs/UI/" + _wc[i].tiredPlants[j].plantName), transform.position, Quaternion.identity, tiredPanel
                    );

                    seedUI.transform.GetChild(3).GetComponent<Text>().text = "x " + _wc[i].tiredPlants[j].amount;
                    Destroy(seedUI.GetComponent<Animator>());
                    seedUI.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    seedUI.transform.localScale = new Vector3(0.8267679f, 0.8267679f, 0.8267679f);
                }

                for (int j = 0; j < _wc[i].witheredPlants.Count; j++)
                {
                    GameObject seedUI = Instantiate(
                        Resources.Load<GameObject>("Prefabs/UI/" + _wc[i].witheredPlants[j].plantName), transform.position, Quaternion.identity, tiredPanel
                    );
                    
                    seedUI.transform.GetChild(3).GetComponent<Text>().text = "x " + _wc[i].witheredPlants[j].amount;
                    Destroy(seedUI.GetComponent<Animator>());
                    seedUI.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                    seedUI.transform.localScale = new Vector3(0.8267679f, 0.8267679f, 0.8267679f);
                }

                break;
            }
        }

        wywPanel.SetActive(true);
        PlantsManager.instance.ClearWorldChanges(world);
    }

    public void SetPlantInAlmanac(PlantAsset pa)
    {
        if (spawnedUIPlant != null)
            Destroy(spawnedUIPlant.gameObject);

        spawnedUIPlant = Instantiate(pa.uiPlant.gameObject, plantSpawn.position, Quaternion.Euler(new Vector3(0f, 0f, 0f))).GetComponent<Plant>();
        spawnedUIPlant.enabled = false;
        spawnedUIPlant.transform.parent = plantSpawn;
        spawnedUIPlant.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

        plantNameT.text = pa.plantName;
        plantBuyPriceT.text = "$ " + pa.buyPrice.ToString("N0");
        plantBackGround.sprite = pa.plantBackg;
        plantDescT.text = pa.plantDescription;
        plantQualityT.text = GetStringedPlantQuality(pa.plantQuality.quality, true);

        PlantProcessAsset ppa = Resources.Load<PlantProcessAsset>("PlantProcessAsset/" + GetStringedPlantQuality(pa.plantQuality.quality, false));

        if (ppa != null)
            plantExtraPercT.text = "% " + ppa.plantPercentageExtra;

        if (pa.canBePlantedIn.Count > 1)
        {
            plantFlowerPotsT.text = GetStringedFlowerPot(pa.canBePlantedIn[0]) + ", ";

            for (int i = 1; i < pa.canBePlantedIn.Count - 1; i++)
                plantFlowerPotsT.text += GetStringedFlowerPot(pa.canBePlantedIn[i]) + ", ";

            plantFlowerPotsT.text += GetStringedFlowerPot(pa.canBePlantedIn[pa.canBePlantedIn.Count - 1]);
        }

        else
            plantFlowerPotsT.text = GetStringedFlowerPot(pa.canBePlantedIn[0]);

        spawnedUIPlant.transform.localScale = new Vector3(-spawnedUIPlant.transform.localScale.x, spawnedUIPlant.transform.localScale.y);
    }

    public void CheckAvailabilityForPinatas(PinataAsset pa)
    {
        int currentlyUnlocked = 0;

        for (int i = 0; i < pa.sizes.Length; i++)
        {
            if (pa.sizes[i].pinataSize == pinataQualitiesChances[currentSizesIndex].sizeToApply)
            {
                for (int j = 0; j < pa.plantsThatCanAppear.Length; j++)
                {   
                    for (int k = 0; k < pa.plantsThatCanAppear[j].plantsOfThatQuality.Length; k++)
                        if (SeedDatabase.instance.PlayerOwnsPlant(pa.plantsThatCanAppear[j].plantsOfThatQuality[k]))
                            currentlyUnlocked++;
                }

                if (currentlyUnlocked < pinataQualitiesChances[currentSizesIndex].minUnlockedPlantsToUse)
                    pinataPriceT.text = "NOT ENOUGH PLANTS!";

                buyPinataCoin.SetActive(currentlyUnlocked >= pinataQualitiesChances[currentSizesIndex].minUnlockedPlantsToUse);
                buyPinataButton.interactable = currentlyUnlocked >= pinataQualitiesChances[currentSizesIndex].minUnlockedPlantsToUse;
            }
        }
    }

    public void CalculatePinataRewards()
    {
        //Iterate through all Pinata Size
        for (int i = 0; i < pinataQualitiesChances.Length; i++)
        {   //Check to only run iteration untill the right Pinata Size
            if (pinataQualitiesChances[i].sizeToApply == allPinatas[currentPinatasIndex].sizes[currentSizesIndex].pinataSize)
            {   //Iterate through all Plant Qualities
                for (int j = 0; j < pinataQualitiesChances[i].chances.Length; j++)
                {   //Get a random bool from Quality's appear chance
                    if (GameHelper.GetBoolFromChance(pinataQualitiesChances[i].chances[j].appearChance))
                    {   //If the random bool is true, the rewards will be chosen for that Quality
                        List<PlantAsset> nonUsed = new List<PlantAsset>();
                        List<PlantAsset> usedRews = new List<PlantAsset>();
                        //Assign Plants to the 'nonUsed' list, only unlocked plants will be added to the list
                        for (int k = 0; k < allPinatas[currentPinatasIndex].plantsThatCanAppear.Length; k++)
                        {
                            if (pinataQualitiesChances[i].chances[j].quality == allPinatas[currentPinatasIndex].plantsThatCanAppear[k].quality)
                            {
                                for (int l = 0; l < allPinatas[currentPinatasIndex].plantsThatCanAppear[k].plantsOfThatQuality.Length; )
                                {
                                    if (SeedDatabase.instance.PlayerOwnsPlant(allPinatas[currentPinatasIndex].plantsThatCanAppear[k].plantsOfThatQuality[l]))
                                    {
                                        nonUsed.Add(allPinatas[currentPinatasIndex].plantsThatCanAppear[k].plantsOfThatQuality[l]);
                                        l++;
                                    }
                                }

                                break;
                            }
                        }
                        //Randomly add plants to the 'usedRews' based on how many Plants can appear
                        for (int k = 0; k < pinataQualitiesChances[i].chances[j].plantAmount; k++)
                        {
                            int plantIdx = Random.Range(0, nonUsed.Count);
                            usedRews.Add(nonUsed[plantIdx]);
                            nonUsed.RemoveAt(plantIdx);
                        }
                        //Assign the Plants on the 'usedRews' to it's respective Quality Reward list, to later be used by the Pinata
                        for (int k = 0; k < usedRews.Count; k++)
                        {
                            int randSeeds = Random.Range(pinataQualitiesChances[i].chances[j].seedsRange.x,
                                pinataQualitiesChances[i].chances[j].seedsRange.y);
                            
                            switch (pinataQualitiesChances[i].chances[j].quality)
                            {
                                case PlantQualityName.Common:
                                    rewComm.Add(new RewardedPlants(PlantQualityName.Common, usedRews[k], randSeeds));
                                break;

                                case PlantQualityName.Rare:
                                    rewRar.Add(new RewardedPlants(PlantQualityName.Rare, usedRews[k], randSeeds));
                                break;

                                case PlantQualityName.Epic:
                                    rewEpc.Add(new RewardedPlants(PlantQualityName.Epic, usedRews[k], randSeeds));
                                break;

                                case PlantQualityName.Legendary:
                                    rewLeg.Add(new RewardedPlants(PlantQualityName.Legendary, usedRews[k], randSeeds));
                                break;
                            }
                        }
                    }
                }

                break;
            }
        }
    }

    public List<List<RewardedPlants>> GetFullPinataRewards()
    {
        List<List<RewardedPlants>> retList = new List<List<RewardedPlants>>();

        retList.Add(rewComm); retList.Add(rewRar); retList.Add(rewEpc); retList.Add(rewLeg);
        return retList;
    }

    void InitPinatasMenu()
    {
        pinataPriceT.text = allPinatas[currentPinatasIndex].sizes[currentSizesIndex].pinataPrice.ToString("N0");

        for (int i = 0; i < pinataQualitiesChances[currentSizesIndex].chances.Length; i++)
        {
            pinatasUI[i].appearChance.text = pinataQualitiesChances[currentSizesIndex].chances[i].quality.ToString() + " - " +
                pinataQualitiesChances[currentSizesIndex].chances[i].appearChance + "%";

            pinatasUI[i].plantAmounts.text = "Plants - " + pinataQualitiesChances[currentSizesIndex].chances[i].plantAmount;
            pinatasUI[i].seedsAmounts.text = "Seeds - " + pinataQualitiesChances[currentSizesIndex].chances[i].seedsRange.x;
        }

        CheckAvailabilityForPinatas(allPinatas[currentPinatasIndex]);
    }

    public void LeftPinatasButton(bool type)
    {
        if (type)
        {
            currentSizesIndex--;

            if (currentSizesIndex < 0)
                currentSizesIndex = 3;

            switch (currentSizesIndex)
            {
                case 0: pinataSizeT.text = "Small"; break;
                case 1: pinataSizeT.text = "Medium"; break;
                case 2: pinataSizeT.text = "Large"; break;
                case 3: pinataSizeT.text = "Extra-Large"; break;
            }

            pinataPriceT.text = allPinatas[currentPinatasIndex].sizes[currentSizesIndex].pinataPrice.ToString("N0");

            for (int i = 0; i < pinataQualitiesChances[currentSizesIndex].chances.Length; i++)
            {
                pinatasUI[i].appearChance.text = pinataQualitiesChances[currentSizesIndex].chances[i].quality.ToString() + " - " +
                    pinataQualitiesChances[currentSizesIndex].chances[i].appearChance + "%";

                pinatasUI[i].plantAmounts.text = "Plants - " + pinataQualitiesChances[currentSizesIndex].chances[i].plantAmount;
                pinatasUI[i].seedsAmounts.text = "Seeds - " + pinataQualitiesChances[currentSizesIndex].chances[i].seedsRange.x;
            }

            CheckAvailabilityForPinatas(allPinatas[currentPinatasIndex]);
        }

        else
        {
            currentPinatasIndex--;

            if (currentPinatasIndex < 0)
                currentPinatasIndex = allPinatas.Length - 1;

            menuPinataImage.sprite = allPinatas[currentPinatasIndex].pinataImage;
            pinataNameT.text = allPinatas[currentPinatasIndex].pinataName;

            SetPinataToSmall();
        }
    }

    public void RightPinatasButton(bool type)
    {
        if (type)
        {
            currentSizesIndex++;

            if (currentSizesIndex > 3)
                currentSizesIndex = 0;

            switch (currentSizesIndex)
            {
                case 0: pinataSizeT.text = "Small"; break;
                case 1: pinataSizeT.text = "Medium"; break;
                case 2: pinataSizeT.text = "Large"; break;
                case 3: pinataSizeT.text = "Extra-Large"; break;
            }

            pinataPriceT.text = allPinatas[currentPinatasIndex].sizes[currentSizesIndex].pinataPrice.ToString("N0");

            for (int i = 0; i < pinataQualitiesChances[currentSizesIndex].chances.Length; i++)
            {
                pinatasUI[i].appearChance.text = pinataQualitiesChances[currentSizesIndex].chances[i].quality.ToString() + " - " +
                    pinataQualitiesChances[currentSizesIndex].chances[i].appearChance + "%";

                pinatasUI[i].plantAmounts.text = "Plants - " + pinataQualitiesChances[currentSizesIndex].chances[i].plantAmount;
                pinatasUI[i].seedsAmounts.text = "Seeds - " + pinataQualitiesChances[currentSizesIndex].chances[i].seedsRange.x;
            }

            CheckAvailabilityForPinatas(allPinatas[currentPinatasIndex]);
        }

        else
        {
            currentPinatasIndex++;

            if (currentPinatasIndex > allPinatas.Length - 1)
                currentPinatasIndex = 0;

            menuPinataImage.sprite = allPinatas[currentPinatasIndex].pinataImage;
            pinataNameT.text = allPinatas[currentPinatasIndex].pinataName;

            SetPinataToSmall();
        }
    }

    void SetPinataToSmall()
    {
        currentSizesIndex = 0;
        pinataSizeT.text = "Small";
        pinataPriceT.text = allPinatas[currentPinatasIndex].sizes[0].pinataPrice.ToString("N0");

        pinataPriceT.text = allPinatas[currentPinatasIndex].sizes[currentSizesIndex].pinataPrice.ToString("N0");

        for (int i = 0; i < pinataQualitiesChances[currentSizesIndex].chances.Length; i++)
        {
            pinatasUI[i].appearChance.text = pinataQualitiesChances[currentSizesIndex].chances[i].quality.ToString() + " - " +
                pinataQualitiesChances[currentSizesIndex].chances[i].appearChance + "%";

            pinatasUI[i].plantAmounts.text = "Plants - " + pinataQualitiesChances[currentSizesIndex].chances[i].plantAmount;
            pinatasUI[i].seedsAmounts.text = "Seeds - " + pinataQualitiesChances[currentSizesIndex].chances[i].seedsRange.x;
        }

        CheckAvailabilityForPinatas(allPinatas[currentPinatasIndex]);
    }

    public void ActivatePinataContinueButton()
    {
        pinataContinueButton.SetActive(true);
    }

    public void ActivatePinataMenu()
    {
        pinataMenu.SetActive(true);
        pinataImage.sprite = creaPinata.sr.sprite;
    }

    public void DeActivatePinataMenu()
    {
        pinataMenu.SetActive(false);
        pinataContinueButton.SetActive(false);
        pinataRewardText.text = "Grab and Squish the Pinata with the Button B and the Trigger!";
        creaPinata.DeleteAllCreatedRewards();
        rewComm.Clear(); rewRar.Clear(); rewEpc.Clear(); rewLeg.Clear();
        Destroy(creaPinata.gameObject);
    }

    public string GetStringedFlowerPot(FlowerPotType fpt)
    {
        switch (fpt)
        {
            case FlowerPotType.Basket: return "BASKET";
            case FlowerPotType.Bronze: return "BRONZE";
            case FlowerPotType.Crystal: return "CRYSTAL WATER";
            case FlowerPotType.DarkEnergy: return "DARK ENERGY";
            case FlowerPotType.FutureTech: return "FUTURE-TECH";
            case FlowerPotType.Golden: return "GOLDEN";
            case FlowerPotType.Holographic: return "HOLOGRAPHIC";
            case FlowerPotType.Jurassic: return "JURASSIC";
            case FlowerPotType.Silver: return "SILVER";
            case FlowerPotType.Terracota: return "TERRACOTA";
            case FlowerPotType.Volcanic: return "VOLCANIC";
        }

        return string.Empty;
    }

    public string GetStringedPlantQuality(PlantQualityName pqd, bool capit)
    {
        if (capit)
        {
            switch (pqd)
            {
                case PlantQualityName.Common: return "COMMON";
                case PlantQualityName.Rare: return "RARE";
                case PlantQualityName.Epic: return "EPIC";
                case PlantQualityName.Legendary: return "LEGENDARY";
                case PlantQualityName.Botanic: return "BOTANIC";
            }
        }

        else
        {
            switch (pqd)
            {
                case PlantQualityName.Common: return "Common";
                case PlantQualityName.Rare: return "Rare";
                case PlantQualityName.Epic: return "Epic";
                case PlantQualityName.Legendary: return "Legendary";
                case PlantQualityName.Botanic: return "Botanic";
            }
        }

        return string.Empty;
    }

    public void SpawnPlantOnHand(Plant plant)
    {
        if (Player.instance.placingFlowerPot == null && seedDatabase.CanPlant(plant.plantData))
        {
            if (player.GetHoldingPlant() == null)
                CreatePlantOnPlayerHand(plant.gameObject);

            else
            {
                if (player.GetHoldingPlant().gameObject.name != plant.gameObject.name)
                {
                    player.DestroyHoldingPlant();
                    CreatePlantOnPlayerHand(plant.gameObject);
                }
            }

            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("selectplant"));
        }

        else
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("cantselect"));
    }

    public void BuyFlowerPot(FlowerPotAsset fpa)
    {
        if (player.CanSpendMoney(fpa.flowerPotPrice))
        {
            seedDatabase.AddFlowerPot(fpa);
            player.SpendMoney(fpa.flowerPotPrice);

            SoundEffectsManager.instance.PlaySoundEffectNC("tap");
        }
    }

    public void BuyPlantAmount(PlantAsset plant)
    {
        if (player.CanSpendMoney(plant.buyPrice))
        {
            seedDatabase.BuyPlant(plant);
            player.SpendMoney(plant.buyPrice);

            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("tap"));
        }

        else
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("cantselect"));
    }

    public void ChangePlantsFromWorld(string world)
    {
        if (world != activePlants)
        {
            foreach (GameObject g in plantSections)
                g.SetActive(false);

            switch (world)
            {
                case "modern": plantSections[0].SetActive(true); break;
                case "jurassic": plantSections[1].SetActive(true); break;
                case "neon": plantSections[2].SetActive(true); break;
                case "dark": plantSections[3].SetActive(true); break;
                case "pirate": plantSections[4].SetActive(true); break;
                case "future": plantSections[5].SetActive(true); break;
                case "lost": plantSections[6].SetActive(true); break;
                case "west": plantSections[7].SetActive(true); break;
                case "beach": plantSections[8].SetActive(true); break;
                case "frostbite": plantSections[9].SetActive(true); break;
                case "egypt": plantSections[10].SetActive(true); break;
                case "present": plantSections[11].SetActive(true); break;
            }
        }

        else
            SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
    }

    public void BuyPinata()
    {   
        if (Player.instance.GetPlayerMoney() >= allPinatas[currentPinatasIndex].sizes[currentSizesIndex].pinataPrice)
        {
            CalculatePinataRewards();
            GameObject pinataG = allPinatas[currentPinatasIndex].pinataGameObject;
            creaPinata = Instantiate(pinataG, pinataG.transform.position, Quaternion.identity).GetComponent<Pinata>();
            creaPinata.SetPinataSize(pinataQualitiesChances[currentSizesIndex].sizeToApply.ToString().ToLower());
            ActivatePinataMenu();
            Player.instance.SpendMoney(allPinatas[currentPinatasIndex].sizes[currentSizesIndex].pinataPrice);
        }

        else
            SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
    }

    void CreatePlantOnPlayerHand(GameObject plant)
    {
        GameObject selectPlant = Instantiate(plant,
            player.leftHand.handInteractor.transform.position, Quaternion.identity
        );

        selectPlant.gameObject.name = selectPlant.gameObject.name.Replace("(Clone)", string.Empty);
        selectPlant.GetComponent<Plant>().SetHandPosition(player.leftHand.handInteractor.transform);
        player.CreatedAPlant(selectPlant.GetComponent<Plant>());
    }

    void CreateFlowerPotOnPlayerHand(GameObject flowerPot)
    {
        GameObject selectFlower = Instantiate(flowerPot,
            player.leftHand.handInteractor.transform.position, Quaternion.identity);

        selectFlower.gameObject.name = selectFlower.gameObject.name.Replace("(Clone)", string.Empty);
        selectFlower.GetComponent<FlowerPot>().SetHandPosition(player.leftHand.handInteractor.transform);
        player.CreatedAFlowerPot(selectFlower.GetComponent<FlowerPot>());
    }
}

[System.Serializable]
public class RewardedPlants
{
    public PlantQualityName quality;
    public PlantAsset plant;
    public int givenSeeds;

    public RewardedPlants (PlantQualityName quality, PlantAsset plant, int givenSeeds)
    {
        this.quality = quality;
        this.plant = plant;
        this.givenSeeds = givenSeeds;
    }
}

[System.Serializable]
public class PinatasUIQuality
{
    public Text appearChance;
    public Text plantAmounts;
    public Text seedsAmounts;
}

[System.Serializable]
public class PinataRewardsPlaceholders
{
    public PinataSizeCategory sizeToApply;
    public int minUnlockedPlantsToUse;
    [NonReorderable]
    public QualityChance[] chances;
}
