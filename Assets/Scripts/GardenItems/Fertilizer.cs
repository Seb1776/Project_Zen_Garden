using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fertilizer : GardenItem
{
    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (GetBelowFlowerPot() != null)
        {
            FlowerPot fp = GetBelowFlowerPot();

            if (fp.GetPlantedPlant() != null && fp.GetPlantedPlant().ExpectedGardenItem() == GardenItemType.Fertilizer && fp.canApplyItem)
            {
                GardenItemSFX();
                fp.GetPlantedPlant().ApplyGardenItem(GardenItemType.Fertilizer);
            }
        }

        base.GardenItemAction(ctx);
    }
}
