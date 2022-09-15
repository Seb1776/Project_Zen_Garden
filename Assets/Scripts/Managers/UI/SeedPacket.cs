using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedPacket : MonoBehaviour
{
    [SerializeField] private PlantAsset plantData;
    public GameObject alamancEquivalent;
    [SerializeField] private bool autoUnlock;
    private GameObject buyButton;
    private SeedDatabase seedDatabase;
    private Image plantImage;
    private List<Image> altPlantImage = new List<Image>();
    private Image plantQualityImage;
    private Text plantName;
    private Text plantAmount;
    private Text plantBuyPriceText;
    private Text plantUnlockPrice;
    private Button plantButton;
    private GameObject buyMoreButton;
    private bool settedData;

    [SerializeField] private Player player;

    public void SeedPacketStart()
    {
        seedDatabase = GameObject.FindGameObjectWithTag("SeedDatabase").GetComponent<SeedDatabase>();
        SetPlantUIData();
        CheckPlantState();

        if (autoUnlock)
        {
            seedDatabase.UnlockPlant(plantData, this);
            alamancEquivalent.SetActive(true);
        }
    }

    void OnEnable()
    {   
        if (settedData)
            CheckForBotanic();
    }

    void SetPlantUIData()
    {
        plantButton = GetComponent<Button>();

        if (transform.GetChild(1).childCount <= 0)
            plantImage = transform.GetChild(1).GetComponent<Image>();
        
        else
        {
            if (transform.GetChild(1).transform.GetChild(0).childCount == 1)
                plantImage = transform.GetChild(1).transform.GetChild(0).GetComponent<Image>();
            
            else
                foreach (Image im in transform.GetChild(1).transform.GetChild(0).GetComponentsInChildren<Image>())
                    altPlantImage.Add(im);
        }

        plantName = transform.GetChild(2).GetComponent<Text>();
        plantQualityImage = transform.GetChild(3).GetComponent<Image>();
        plantAmount = transform.GetChild(4).GetComponent<Text>();
        buyButton = transform.GetChild(5).gameObject;
        buyMoreButton = transform.GetChild(6).gameObject;
        plantBuyPriceText = buyMoreButton.transform.GetChild(1).GetComponent<Text>();
        plantUnlockPrice = buyButton.transform.GetChild(1).GetComponent<Text>();
        plantQualityImage.GetComponent<Animator>().enabled = false;
        plantQualityImage.color = plantData.plantQuality.qualityColor;
        CheckForBotanic();

        settedData = true;
    }

    void CheckForBotanic()
    {
        if (plantData.plantQuality.quality == PlantQualityName.Botanic)
        {
            plantQualityImage.GetComponent<Animator>().enabled = true;
            plantQualityImage.GetComponent<Animator>().SetTrigger("rainbow");
        }
    }

    public void BuyPlant()
    {
        if (player.CanSpendMoney(plantData.unlockPrice))
        {
            player.SpendMoney(plantData.unlockPrice);
            seedDatabase.UnlockPlant(plantData, this);
            alamancEquivalent.SetActive(true);
            CheckPlantState();
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("tap"));
        }
        
        else
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("cantselect"));
    }

    void CheckPlantState()
    {
        if (seedDatabase.PlayerOwnsPlant(plantData))
        {
            if (seedDatabase.GetPlantInList(plantData).amount > 0)
            {
                if (altPlantImage.Count <= 0) plantImage.color = Color.white;
                else foreach (Image i in altPlantImage) i.color = Color.white;
            }
            
            else
            {
                if (altPlantImage.Count <= 0) plantImage.color = new Color(.5f, .5f, .5f, 1f);
                else foreach (Image i in altPlantImage) i.color = new Color(.5f, .5f, .5f, 1f);
            }

            plantName.text = plantData.plantName;
            plantAmount.gameObject.SetActive(true);
            plantButton.enabled = true;
            buyButton.gameObject.SetActive(false);
            buyMoreButton.gameObject.SetActive(true);
            plantBuyPriceText.text = "$ " + plantData.buyPrice.ToString("0,0");
            UpdatePlantAmount();
        }

        else
        {
            if (altPlantImage.Count <= 0) plantImage.color = Color.black;
            else foreach (Image i in altPlantImage) i.color = Color.black;

            plantName.text = "???";
            plantButton.enabled = false;
            plantAmount.gameObject.SetActive(false);
            buyMoreButton.gameObject.SetActive(false);
            plantUnlockPrice.text = "$ " + plantData.unlockPrice.ToString("0,0");
        }
    }

    public void UpdatePlantAmount()
    {
        plantAmount.text = (seedDatabase.GetPlantInList(plantData).amount).ToString();
        
        if (seedDatabase.GetPlantInList(plantData).amount > 0)
            if (altPlantImage.Count == 0) plantImage.color = Color.white;
            else foreach (Image i in altPlantImage) i.color = Color.white;
            
        else
            if (altPlantImage.Count == 0) plantImage.color = new Color(.5f, .5f, .5f, 1f);
            else foreach (Image i in altPlantImage) i.color = new Color(.5f, .5f, .5f, 1f);
    }
}
