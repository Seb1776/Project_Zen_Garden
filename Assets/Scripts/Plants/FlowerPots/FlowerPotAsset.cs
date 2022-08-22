using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Flower Pot Asset", menuName = "Plants/Flower Pot", order = 0)]
public class FlowerPotAsset : ScriptableObject 
{
    public string flowerPotName;
    public FlowerPotType flowerPotType;
    public List<GameWorlds> canBeUsedIn = new List<GameWorlds>();
    public int flowerPotPrice;
}
