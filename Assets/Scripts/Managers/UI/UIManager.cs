using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

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
    private Pinata creaPinata;
    [SerializeField] private string activePlants;
    [Header("Almanac UI")]
    [SerializeField] private Text plantNameT;
    [SerializeField] private Text plantBuyPriceT, plantSellPriceT, plantExtraPercT, plantQualityT, plantFlowerPotsT, plantDescT;
    [SerializeField] private Image plantBackGround;
    [SerializeField] private Transform plantSpawn;
    [Header("Pinatas UI")]
    [SerializeField] private GameObject pinataMenu;
    [SerializeField] private Image pinataImage;
    [SerializeField] private GameObject pinataContinueButton;
    [SerializeField] [NonReorderable] private PinatasUI[] pinatasUI;
    public Text pinataRewardText;
    private Plant spawnedUIPlant;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
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
        plantSellPriceT.text = "$ " + pa.revenuePrice.ToString("N0");
        plantBackGround.sprite = pa.plantBackg;
        plantDescT.text = pa.plantDescription;
        plantQualityT.text = GetStringedPlantQuality(pa.plantQuality.quality, true);

        PlantProcessAsset ppa = Resources.Load<PlantProcessAsset>("PlantProcessAsset/" + GetStringedPlantQuality(pa.plantQuality.quality, false));
        Debug.Log(ppa + " " + ("PlantProcessAsset/" + GetStringedPlantQuality(pa.plantQuality.quality, false)));


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

    public void CheckAvailabilityForPinatas()
    {
        for (int i = 0; i < pinatasUI.Length; i++)
        {
            for (int j = 0; j < pinatasUI[i].pinataAsset.sizes.Length; j++)
            {
                int currentlyUnlocked = 0;

                for (int k = 0; k < pinatasUI[i].pinataAsset.plantsThatCanAppear.Length; k++)
                {
                    if (SeedDatabase.instance.PlayerOwnsPlant(pinatasUI[i].pinataAsset.plantsThatCanAppear[k]))
                    {
                        currentlyUnlocked++;
                    }
                }

                if (currentlyUnlocked >= pinatasUI[i].pinataAsset.sizes[j].minUnlockedPlantsToUse)
                    pinatasUI[i].SetAvailability(j, true);
                
                else
                    pinatasUI[i].SetAvailability(j, false);
            }
        }
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

    public void SelectSPinata(PinataAsset pa) => CreatePinata("s", pa.pinataGameObject);

    public void SelectMPinata(PinataAsset pa) => CreatePinata("m", pa.pinataGameObject);

    public void SelectLPinata(PinataAsset pa) => CreatePinata("l", pa.pinataGameObject);

    public void SelectXLPinata(PinataAsset pa) => CreatePinata("xl", pa.pinataGameObject);

    public void CreatePinata(string pinataSize, GameObject pinataG)
    {
        creaPinata = Instantiate(pinataG, pinataG.transform.position, Quaternion.identity).GetComponent<Pinata>();
        creaPinata.SetPinataSize(pinataSize);
        ActivatePinataMenu();
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
public class PinatasUI
{
    public PinataAsset pinataAsset;
    [NonReorderable]
    public PinatasButtons[] buttons;

    public void SetAvailability(int index, bool set)
    {
        buttons[index].buyButton.interactable = set;
        buttons[index].coin.SetActive(set);

        if (set)
            buttons[index].availabilityText.text = pinataAsset.sizes[index].pinataPrice.ToString("N0");
        else
            buttons[index].availabilityText.text = "Not Enough Plants!";
    }
}

[System.Serializable]
public class PinatasButtons
{
    public Button buyButton;
    public Text availabilityText;
    public GameObject coin;
}
