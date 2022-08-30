using UnityEngine;

[CreateAssetMenu(fileName = "New Pinata Asset", menuName = "Pinata/Create Pinata")]
public class PinataAsset : ScriptableObject 
{
    [NonReorderable] public PinataSize[] sizes;
    public Sprite pinataImage;
    public GameObject pinataGameObject;
    public PlantsDividedQuality[] plantsThatCanAppear;
}

[System.Serializable]
public class PinataSize
{
    public int pinataPrice;
    public int minUnlockedPlantsToUse;
    public PinataSizeCategory pinataSize;
    public Vector2Int squishesRange;
    /*[NonReorderable]
    public QualityChance[] qualitiesToAppear;*/
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
