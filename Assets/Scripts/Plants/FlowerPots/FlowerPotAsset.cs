using UnityEngine;

[CreateAssetMenu(fileName = "New Flower Pot Asset", menuName = "Plants/Flower Pot", order = 0)]
public class FlowerPotAsset : ScriptableObject 
{
    public FlowerPotType flowerPotType;
    public int flowerPotPrice;
}
