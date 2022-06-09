using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedPacket : MonoBehaviour
{
    [SerializeField] private PlantAsset plantData;
    private GameObject buyButton;
    private SeedDatabase seedDatabase;
    private Image plantImage;
    private Text plantName;
    private Text plantAmount;
    private Text plantBuyPriceText;
    private Text plantUnlockPrice;
    private Button plantButton;
    private GameObject buyMoreButton;

    [SerializeField] private Player player;

    void Awake()
    {
        seedDatabase = GameObject.FindGameObjectWithTag("SeedDatabase").GetComponent<SeedDatabase>();
    }

    void Start()
    {
        SetPlantUIData();
        CheckPlantState();
    }

    void SetPlantUIData()
    {
        plantButton = GetComponent<Button>();
        plantImage = transform.GetChild(1).GetComponent<Image>();
        plantName = transform.GetChild(2).GetComponent<Text>();
        plantAmount = transform.GetChild(4).GetComponent<Text>();
        buyButton = transform.GetChild(5).gameObject;
        buyMoreButton = transform.GetChild(6).gameObject;
        plantBuyPriceText = buyMoreButton.transform.GetChild(1).GetComponent<Text>();
        plantUnlockPrice = buyButton.transform.GetChild(1).GetComponent<Text>();
    }

    public void BuyPlant()
    {
        if (player.currentPlayerCoins >= plantData.unlockPrice)
        {
            player.currentPlayerCoins -= plantData.unlockPrice;
            seedDatabase.UnlockPlant(plantData, this);
            CheckPlantState();
        }
    }

    void CheckPlantState()
    {
        if (seedDatabase.PlayerOwnsPlant(plantData))
        {
            if (seedDatabase.GetPlantInList(plantData).amount > 0)
                plantImage.color = Color.white;
            
            else
                plantImage.color = new Color(.5f, .5f, .5f, 1f);

            plantName.text = plantData.plantName;
            plantAmount.gameObject.SetActive(true);
            plantButton.enabled = true;
            buyButton.gameObject.SetActive(false);
            buyMoreButton.gameObject.SetActive(true);
            plantBuyPriceText.text = "$ " + plantData.buyPrice.ToString();
            UpdatePlantAmount();
        }

        else
        {
            plantImage.color = Color.black;
            plantName.text = "???";
            plantButton.enabled = false;
            plantAmount.gameObject.SetActive(false);
            buyMoreButton.gameObject.SetActive(false);
            plantUnlockPrice.text = "$ " + plantData.unlockPrice.ToString();
        }
    }

    public void UpdatePlantAmount()
    {
        plantAmount.text = (seedDatabase.GetPlantInList(plantData).amount).ToString();
        
        if (seedDatabase.GetPlantInList(plantData).amount > 0)
            plantImage.color = Color.white;
            
        else
            plantImage.color = new Color(.5f, .5f, .5f, 1f);
    }
}
