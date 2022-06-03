using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WateringCan : GardenItem
{
    [SerializeField] private Vector2 acceptableRotationRange;
    [SerializeField] private float waterEffectDuration;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        if (transform.eulerAngles.z >= acceptableRotationRange.x && transform.eulerAngles.z <= acceptableRotationRange.y)
            canUseItem = true;
        
        else
            canUseItem = false;

        base.Update();
    }

    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (GetBelowFlowerPot() != null)
        {   
            FlowerPot fp = GetBelowFlowerPot();

            if (fp.GetPlantedPlant() != null && fp.GetPlantedPlant().ExpectedGardenItem() == GardenItemType.Water)
            {
                StartCoroutine(fp.TriggerWaterEffect(waterEffectDuration));
                fp.GetPlantedPlant().ApplyGardenItem(GardenItemType.Water);
            }
        }
        
        base.GardenItemAction(ctx);
    }
}
