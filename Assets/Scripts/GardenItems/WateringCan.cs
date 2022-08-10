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

    public override void Start()
    {
        foreach (GardenItem gi in SeedDatabase.instance.waterUI.items)
            if (gi.gameObject.transform.parent.gameObject != null && gi.gameObject.transform.parent.gameObject.activeSelf)
                Debug.Log(gi.gameObject.transform.parent.gameObject);

        base.Start();
    }

    public override void Update()
    {
        if (transform.eulerAngles.z >= acceptableRotationRange.x && transform.eulerAngles.z <= acceptableRotationRange.y)
            canUseItem = true;
        
        else
            canUseItem = false;
        
        DetectEffect();

        base.Update();
    }

    public override void DetectEffect()
    {
        if (GetBelowFlowerPot() != null)
        {
            detectedPot = GetBelowFlowerPot();

            if (detectedPot.canApplyItem)
            {
                if (detectedPot.GetPlantedPlant() != null && canUseItem)
                {
                    Plant p = detectedPot.GetPlantedPlant();

                    if (p.ExpectedGardenItem() == GardenItemType.Water)
                        detectedPot.outline.ChangeOutlineColor(Color.green, true);
                    
                    else
                        detectedPot.outline.ChangeOutlineColor(Color.red, true);
                }

                else
                    detectedPot.outline.ChangeOutlineColor(Color.white, false);
            }
        }

        else
        {   
            if (detectedPot != null && detectedPot.GetPlantedPlant() != null)
            {
                if (!detectedPot.GetPlantedPlant().fullyGrown)
                {
                    detectedPot.outline.ChangeOutlineColor(Color.white, false);
                    detectedPot = null;
                }

                else if (detectedPot.GetPlantedPlant() != null && detectedPot.GetPlantedPlant().fullyGrown)
                {
                    detectedPot.outline.ChangeOutlineColor(new Color(252f / 256f, 157f / 256f, 3f / 256f), true);
                    detectedPot = null;
                }
            }
        }

        base.DetectEffect();
    }

    public override void CheckForUsability()
    {
        if (!SeedDatabase.instance.GardenIsAvailable(GardenItemType.Water))
        {
            coll.enabled = false;
            grab.enabled = false;
            SetColors(new Color(.5f, .5f, .5f, 1f));
        }

        else
        {
            coll.enabled = true;
            grab.enabled = true;
            SetColors(new Color(1f, 1f, 1f, 1f));
        }

        base.CheckForUsability();
    }

    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (GetBelowFlowerPot() != null && canUseItem)
        {   
            FlowerPot fp = GetBelowFlowerPot();

            if (fp.canApplyItem)
            {
                if (fp.GetPlantedPlant() != null && fp.GetPlantedPlant().ExpectedGardenItem() == GardenItemType.Water)
                {
                    fp.canApplyItem = false;
                    StartCoroutine(fp.TriggerWaterEffect(waterEffectDuration));
                    GardenItemSFX();
                    fp.GetPlantedPlant().ApplyGardenItem(GardenItemType.Water);
                    SeedDatabase.instance.GardenUse(GardenItemType.Water, false);
                    
                    foreach (GardenItem gi in SeedDatabase.instance.waterUI.items)
                        //if (gi.gameObject.transform.parent.gameObject != null && gi.gameObject.transform.parent.gameObject.activeSelf)
                            gi.CheckForUsability();
                }
            }
        }
        
        base.GardenItemAction(ctx);
    }
}
