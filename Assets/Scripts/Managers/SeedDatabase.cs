using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedDatabase : MonoBehaviour
{
    public static SeedDatabase instance;

    [NonReorderable] public FlowerPots[] flowerPots;
    [SerializeField] private FlowerPotHolder[] holders;
    [SerializeField] private List<UnlockedSeeds> unlockedSeeds = new List<UnlockedSeeds>();

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        foreach (FlowerPots fps in flowerPots)
            fps.InitUI();
    }

    void Update()
    {
        foreach (FlowerPots fps in flowerPots)
            fps.SetUI();
    }

    public FlowerPots GetFlowerPotData(FlowerPotAsset fpa)
    {
        foreach (FlowerPots fp in flowerPots)
        {
            if (fp.pot.flowerPotAsset == fpa)
            {
                return fp;
            }
        }

        return null;
    }

    public void TriggerHolders(bool act)
    {
        foreach (FlowerPotHolder fph in holders)
            fph.canShowEffect = act;
    }

    public bool CanUseFlowerPot(FlowerPotAsset fpa)
    {
        if (GetFlowerPotData(fpa).amount > 0 && fpa.canBeUsedIn.Contains(MusicManager.instance.GetCurrentMusic().world))
            return true;

        SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
        return false;
    }

    public void AddFlowerPot(FlowerPotAsset fpa)
    {
        if (GetFlowerPotData(fpa) != null)
        {
            FlowerPots fp = GetFlowerPotData(fpa);
            fp.amount++;
        }
    }

    public void UseFlowerPot(FlowerPotAsset fpa)
    {
        if (GetFlowerPotData(fpa) != null)
        {
            FlowerPots fp = GetFlowerPotData(fpa);
            fp.amount--;
        }
    }

    public void UnlockPlant(PlantAsset plant, SeedPacket ui)
    {
        UnlockedSeeds newPlant = new UnlockedSeeds(plant, 1, ui);
        unlockedSeeds.Add(newPlant);
    }

    public void BuyPlant(PlantAsset plant)
    {
        if (PlayerOwnsPlant(plant))
        {
            UnlockedSeeds plantTo = GetPlantInList(plant);
            plantTo.amount++;
            plantTo.uiPacket.UpdatePlantAmount();
        }
    }

    public void UsePlant(PlantAsset plant)
    {
        if (PlayerOwnsPlant(plant))
        {   
            UnlockedSeeds plantTo = GetPlantInList(plant);
            plantTo.amount--;
            plantTo.uiPacket.UpdatePlantAmount();
        }
    }

    public bool CanPlant(PlantAsset plant)
    {
        UnlockedSeeds unlocked = null;

        if (PlayerOwnsPlant(plant))
            unlocked = GetPlantInList(plant);

        return PlayerOwnsPlant(plant) && unlocked.amount > 0;
    }

    public bool PlayerOwnsPlant(PlantAsset plant)
    {
        for (int i = 0; i < unlockedSeeds.Count; i++)
            if (unlockedSeeds[i].plant == plant)
                return true;

        return false;
    }

    public UnlockedSeeds GetPlantInList(PlantAsset plant)
    {
        for (int i = 0; i < unlockedSeeds.Count; i++)
            if (unlockedSeeds[i].plant == plant)
                return unlockedSeeds[i];
        
        return null;
    }
}

[System.Serializable]
public class UnlockedSeeds
{
    public PlantAsset plant;
    public SeedPacket uiPacket;
    public int amount;

    public UnlockedSeeds(PlantAsset plant, int amount, SeedPacket uiPacket)
    {
        this.plant = plant;
        this.amount = amount;
        this.uiPacket = uiPacket;
    }
}

[System.Serializable]
public class FlowerPots
{
    public FlowerPot pot;
    public bool unlocked;
    public int amount;
    public Button buyButton;
    public GameObject coin, cantUseText, getButton;
    public Text boughtAmount;
    public Text priceText;

    public void InitUI()
    {
        coin.SetActive(pot.flowerPotAsset.canBeUsedIn.Contains(MusicManager.instance.GetCurrentMusic().world));

        if (pot.flowerPotAsset.canBeUsedIn.Contains(MusicManager.instance.GetCurrentMusic().world))
        {
            buyButton.interactable = true;
            cantUseText.SetActive(false);
            getButton.SetActive(true);
            priceText.text = "$ " + pot.flowerPotAsset.flowerPotPrice.ToString("0,0");
        }
        
        else
        {
            buyButton.interactable = false;
            cantUseText.SetActive(true);
            getButton.SetActive(false);
            priceText.text = string.Empty;
        }
    }

    public void SetUI()
    {
        boughtAmount.text = "x" + amount;
    }
}
