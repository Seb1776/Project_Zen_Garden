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
