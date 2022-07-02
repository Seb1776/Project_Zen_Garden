using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private SeedDatabase seedDatabase;
    [Header("UI")]
    [SerializeField] private Text coins;

    void Update()
    {
        PlayerUI();
    }

    void PlayerUI()
    {
        coins.text = "$ " + player.currentPlayerCoins;
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
                if (Player.instance.currentPlayerCoins >= SeedDatabase.instance.waterUI.gardenPrice &&
                    SeedDatabase.instance.GardenIsBuyable(GardenItemType.Water))
                {
                    SeedDatabase.instance.GardenUse(GardenItemType.Water, true);

                    foreach (GardenItem gi in SeedDatabase.instance.waterUI.items)
                        gi.CheckForUsability();

                    Player.instance.currentPlayerCoins -= SeedDatabase.instance.waterUI.gardenPrice;
                    SoundEffectsManager.instance.PlaySoundEffectNC("money");
                }

                else
                    SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
            break;

            case GardenItemType.Compost:
                if (Player.instance.currentPlayerCoins >= SeedDatabase.instance.compostUI.gardenPrice &&
                    SeedDatabase.instance.GardenIsBuyable(GardenItemType.Compost))
                {
                    SeedDatabase.instance.GardenUse(GardenItemType.Compost, true);

                    foreach (GardenItem gi in SeedDatabase.instance.compostUI.items)
                        gi.CheckForUsability();

                    Player.instance.currentPlayerCoins -= SeedDatabase.instance.compostUI.gardenPrice;
                    SoundEffectsManager.instance.PlaySoundEffectNC("money");
                }

                else
                    SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
            break;

            case GardenItemType.Fertilizer:
                if (Player.instance.currentPlayerCoins >= SeedDatabase.instance.fertilizerUI.gardenPrice &&
                    SeedDatabase.instance.GardenIsBuyable(GardenItemType.Fertilizer))
                {
                    SeedDatabase.instance.GardenUse(GardenItemType.Fertilizer, true);

                    foreach (GardenItem gi in SeedDatabase.instance.fertilizerUI.items)
                        gi.CheckForUsability();

                    Player.instance.currentPlayerCoins -= SeedDatabase.instance.fertilizerUI.gardenPrice;
                    SoundEffectsManager.instance.PlaySoundEffectNC("money");
                }

                else
                    SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
            break;

            case GardenItemType.Music:
                if (Player.instance.currentPlayerCoins >= SeedDatabase.instance.phonographUI.gardenPrice &&
                    SeedDatabase.instance.GardenIsBuyable(GardenItemType.Music))
                {
                    SeedDatabase.instance.GardenUse(GardenItemType.Music, true);

                    foreach (GardenItem gi in SeedDatabase.instance.phonographUI.items)
                        gi.CheckForUsability();

                    Player.instance.currentPlayerCoins -= SeedDatabase.instance.phonographUI.gardenPrice;
                    SoundEffectsManager.instance.PlaySoundEffectNC("money");
                }

                else
                    SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
            break;
        }
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
        if (player.currentPlayerCoins >= fpa.flowerPotPrice)
        {
            seedDatabase.AddFlowerPot(fpa);
            player.currentPlayerCoins -= fpa.flowerPotPrice;

            SoundEffectsManager.instance.PlaySoundEffectNC("tap");
        }
    }

    public void BuyPlantAmount(PlantAsset plant)
    {
        if (player.currentPlayerCoins >= plant.buyPrice)
        {
            seedDatabase.BuyPlant(plant);
            player.currentPlayerCoins -= plant.buyPrice;

            if (player.currentPlayerCoins < 0)
                player.currentPlayerCoins = 0;

            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("tap"));
        }

        else
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("cantselect"));
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
