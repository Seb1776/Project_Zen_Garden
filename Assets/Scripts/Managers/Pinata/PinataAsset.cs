using UnityEngine;

[CreateAssetMenu(fileName = "New Pinata Asset", menuName = "Pinata/Create Pinata")]
public class PinataAsset : ScriptableObject 
{
    [NonReorderable] public PinataSize[] sizes;
    public Sprite pinataImage;
    public string pinataName;
    public PlantAsset[] allPlantsFromPinata;
    public bool setted;
    public GameObject pinataGameObject;
    [NonReorderable]
    public PlantsDividedQuality[] plantsThatCanAppear;
}

[System.Serializable]
public class PinataSize
{
    public int pinataPrice;
    public PinataSizeCategory pinataSize;
    public Vector2Int squishesRange;
}

[System.Serializable]
public class PlantsDividedQuality
{
    public PlantQualityName quality;
    public PlantAsset[] plantsOfThatQuality;
}

[System.Serializable]
public class QualityChance
{
    public PlantQualityName quality;
    public int plantAmount;
    public Vector2Int seedsRange;
    public float appearChance;
}

public enum PinataSizeCategory
{
    S, M, L, XL
}
