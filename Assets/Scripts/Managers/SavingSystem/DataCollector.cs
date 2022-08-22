using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    public Player playerData;
    public List<GardenTable> worldTables;

    public void AddFlowerPot(GameWorlds world, FlowerPot flowerPot)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            _gt.AddFlowerPot(flowerPot.flowerPotAsset.flowerPotName, flowerPot.inPositionOfHolder.holderIdx);
        }
    }

    public void RemoveFlowerPot(GameWorlds world, FlowerPot flowerPot)
    {
        if (GetTableFromWorld(world) != null)
        {
            GardenTable _gt = GetTableFromWorld(world);
            _gt.RemoveFlowerPot(flowerPot.inPositionOfHolder.holderIdx);
        }
    }

    public void AddPlant(GameWorlds world, FlowerPot flowerPot, Plant plant)
    {

    }

    public GardenTable GetTableFromWorld(GameWorlds _world)
    {
        for (int i = 0; i < worldTables.Count; i++)
            if (_world == worldTables[i].correspondingWorld)
                return worldTables[i];

        return null;
    }
}

[System.Serializable]
public class GardenTable
{
    public GameWorlds correspondingWorld;
    public List<FlowerPotSpot> flowerPotsOnTable;

    public void AddFlowerPot(string flowerPot, int flowerPotIdx)
    {
        for (int i = 0; i < flowerPotsOnTable.Count; i++)
            if (i == flowerPotIdx)
                flowerPotsOnTable[i].flowerPotInSpace = flowerPot;
    }

    public void RemoveFlowerPot(int flowerPotIdx)
    {
        for (int i = 0; i < flowerPotsOnTable.Count; i++)
            if (i == flowerPotIdx)
                flowerPotsOnTable[i].flowerPotInSpace = string.Empty;
    }
}

[System.Serializable]
public class FlowerPotSpot
{
    public string flowerPotInSpace = string.Empty;
    public int flowerPotIndex = -1;
    public PlantSpot plantInSpace;

    public void AddPlant(Plant plant)
    {
        plantInSpace.plantName = plant.plantData.name;
        plantInSpace.plantLastRequire = plant.ExpectedGardenItem().ToString();
    }
}

[System.Serializable]
public class PlantSpot
{
    public string plantName;
    public string plantLastRequire;
    public int waterTotalUses, compostTotalUses, fertilizerTotalUses, phonographTotalUses;
    public int waterCurrentUses, compostCurrentUses, fertilizerCurrenmtUses, phonographCurrentUses;
    public bool plantIsGrown;
}
