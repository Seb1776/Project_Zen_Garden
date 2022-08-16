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
    public GardenAmount waterUI, compostUI, fertilizerUI, phonographUI;
    [SerializeField] private GameObject jurassicBlock;
    [SerializeField] private Button[] disablableTutorialSeeds;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        foreach (FlowerPots fps in flowerPots)
            fps.InitUI();
        
        waterUI.UpdateUI();
        compostUI.UpdateUI();
        fertilizerUI.UpdateUI();
        phonographUI.UpdateUI();
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

    public void GardenUse(GardenItemType git, bool add)
    {
        switch (git)
        {
            case GardenItemType.Water:
                if (add) waterUI.RefillAmount();
                else waterUI.RemoveAmount();

                waterUI.UpdateUI();
            break;

            case GardenItemType.Compost:
                if (add) compostUI.AddAmount();
                else compostUI.RemoveAmount();

                compostUI.UpdateUI();
            break;

            case GardenItemType.Fertilizer:
                if (add) fertilizerUI.AddAmount();
                else fertilizerUI.RemoveAmount();

                fertilizerUI.UpdateUI();
            break;

            case GardenItemType.Music:
                if (add) phonographUI.AddAmount();
                else phonographUI.RemoveAmount();

                phonographUI.UpdateUI();
            break;
        }
    }

    public bool GardenIsAvailable(GardenItemType git)
    {
        switch (git)
        {
            case GardenItemType.Water:
                return waterUI.IsAvaible();

            case GardenItemType.Compost:
                return compostUI.IsAvaible();

            case GardenItemType.Fertilizer:
                return fertilizerUI.IsAvaible();

            case GardenItemType.Music:
                return phonographUI.IsAvaible();
        }

        return false;
    }

    public bool GardenIsBuyable(GardenItemType git)
    {
        switch (git)
        {
            case GardenItemType.Water:
                return waterUI.IsBuyable();

            case GardenItemType.Compost:
                return compostUI.IsBuyable();

            case GardenItemType.Fertilizer:
                return fertilizerUI.IsBuyable();

            case GardenItemType.Music:
                return phonographUI.IsBuyable();
        }

        return false;
    }

    public void TriggerHolders(bool act)
    {
        foreach (FlowerPotHolder fph in holders)
        {
            fph.canShowEffect = act;
            if (!act) fph.outline.ChangeOutlineColor(Color.white, false);
        }
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

    public int GetTotalUnlockedPlants()
    {
        return unlockedSeeds.Count;
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

        if (GameManager.instance.onTutorial && GetTotalUnlockedPlants() >= 5)
        {
            jurassicBlock.SetActive(false);
            SoundEffectsManager.instance.PlaySoundEffectNC("prize");
        }
    }

    public void IgnorPosOfGardenItems(bool ignore, bool updatePos = false)
    {
        foreach (GardenItem gi in waterUI.items)
        {
            gi.ignorePos = ignore;

            if (updatePos) gi.UpdateReturnPosition();
        }

        foreach (GardenItem gi in compostUI.items)
        {
            gi.ignorePos = ignore;

            if (updatePos) gi.UpdateReturnPosition();
        }

        foreach (GardenItem gi in fertilizerUI.items)
        {
            gi.ignorePos = ignore;

            if (updatePos) gi.UpdateReturnPosition();
        }

        foreach (GardenItem gi in phonographUI.items)
        {
            gi.ignorePos = ignore;

            if (updatePos) gi.UpdateReturnPosition();
        }
    }

    public void TutorialPlantsSet(bool activate)
    {
       foreach (Button b in disablableTutorialSeeds)
            b.interactable = activate;
    }

    public void BuyPlant(PlantAsset plant, int specAmount = 0)
    {
        if (PlayerOwnsPlant(plant))
        {
            UnlockedSeeds plantTo = GetPlantInList(plant);

            if (specAmount == 0)
                plantTo.amount++;
            
            else
                plantTo.amount += specAmount;

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

[System.Serializable]
public class GardenAmount
{
    public int gardenPrice;
    public int maxAmount, currentAmount;
    public bool usesText;
    public Text priceText;
    public GardenItem[] items;
    public GardenAmountUI[] UI;

    public void AddAmount()
    {
        if (currentAmount < maxAmount)
            currentAmount++;
    }

    public void RefillAmount()
    {
        currentAmount = maxAmount;
    }

    public void RemoveAmount()
    {
        if (currentAmount > 0)
            currentAmount--;
    }

    public virtual void UpdateUI()
    {
        priceText.text = "$ " + gardenPrice.ToString("0,0");

        foreach (GardenAmountUI gaui in UI)
        {
            if (!usesText)
            {
                gaui.sliderAmount.maxValue = maxAmount;
                gaui.sliderAmount.value = currentAmount;
                gaui.sliderAmount.minValue = 0;
            }

            else gaui.amountText.text = "x" + currentAmount;
        }
    }

    public bool IsAvaible()
    {
        return currentAmount > 0 && currentAmount <= maxAmount;
    }

    public bool IsBuyable()
    {
        return currentAmount < maxAmount;
    }
}

[System.Serializable]
public class GardenAmountUI
{
    public Text amountText;
    public Slider sliderAmount;
}
