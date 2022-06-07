using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SeedPacket : MonoBehaviour
{
    [SerializeField] private PlantAsset plantData;
    [SerializeField] private GameObject buyButton;
    private SeedDatabase seedDatabase;
    private Image plantImage;
    private Text plantName;
    private Text plantAmount;
    private Button plantButton;

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
    }

    public void BuyPlant()
    {
        seedDatabase.UnlockPlant(plantData);
        CheckPlantState();
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
            plantButton.gameObject.SetActive(false);
            UpdatePlantAmount();
        }

        else
        {
            plantImage.color = Color.black;
            plantName.text = "???";
            plantButton.enabled = false;
            plantAmount.gameObject.SetActive(false);
        }
    }

    void UpdatePlantAmount()
    {
        plantAmount.text = (seedDatabase.GetPlantInList(plantData).amount).ToString();
    }
}
