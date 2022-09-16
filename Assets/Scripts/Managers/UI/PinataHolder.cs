using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PinataHolder : MonoBehaviour
{
    [SerializeField] private PinataAsset pinataData;
    [SerializeField] private PinataSizePrice[] pinataSizes;
    [SerializeField] private Image pinataImage;
}

[System.Serializable]
public class PinataSizePrice
{
    public Text size, seeds, plants, price;
}
