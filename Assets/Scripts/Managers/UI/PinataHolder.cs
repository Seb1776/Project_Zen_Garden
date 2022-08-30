using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PinataHolder : MonoBehaviour
{
    [SerializeField] private PinataAsset pinataData;
    [SerializeField] private PinataSizePrice[] pinataSizes;
    [SerializeField] private Image pinataImage;

    void Start()
    {
        SetUI();
    }

    void SetUI()
    {
        /*pinataImage.sprite = pinataData.pinataImage;

        for (int i = 0; i < pinataSizes.Length; i++)
        {
            if (i == 0) pinataSizes[i].size.text = "Size: Small";
            else if (i == 1) pinataSizes[i].size.text = "Size: Medium";
            else if (i == 2) pinataSizes[i].size.text = "Size: Large";
            else if (i == 3) pinataSizes[i].size.text = "Size: XL";

            pinataSizes[i].plants.text = "Plants: " + pinataData.sizes[i].plantsToAppearRange.x + " - " + pinataData.sizes[i].plantsToAppearRange.y;
            pinataSizes[i].seeds.text = "Seeds: " + pinataData.sizes[i].seedsToGiveRange.x + " - " + pinataData.sizes[i].seedsToGiveRange.y;
            pinataSizes[i].price.text = "$ " + pinataData.sizes[i].pinataPrice.ToString("N0");
        }*/
    }
}

[System.Serializable]
public class PinataSizePrice
{
    public Text size, seeds, plants, price;
}
