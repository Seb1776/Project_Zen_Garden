using UnityEngine;

[CreateAssetMenu(fileName = "New Pinata Asset", menuName = "Pinata/Create Pinata")]
public class PinataAsset : ScriptableObject 
{
    [NonReorderable] public PinataSize[] sizes;
    public Sprite pinataImage;
    public GameObject pinataGameObject;
    public PlantAsset[] plantsThatCanAppear;
    public int minUnlockedPlantsToUse;
}

[System.Serializable]
public class PinataSize
{
    public int pinataPrice;
    public PinataSizeCategory pinataSize;
    public Vector2Int squishesRange;
    public Vector2Int plantsToAppearRange;
    public Vector2Int seedsToGiveRange;
}

public enum PinataSizeCategory
{
    S, M, L, XL
}
