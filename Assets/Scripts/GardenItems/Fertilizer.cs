using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Fertilizer : GardenItem
{
    private Animator animator;

    public override void Start()
    {
        animator = GetComponent<Animator>();

        base.Start();
    }

    public override void Update()
    {
        DetectEffect();

        base.Update();
    }

    public override void CheckForUsability()
    {
        if (!SeedDatabase.instance.GardenIsAvailable(GardenItemType.Fertilizer))
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

    public override void DetectEffect()
    {
        if (GetBelowFlowerPot() != null)
        {
            detectedPot = GetBelowFlowerPot();

            if (detectedPot.canApplyItem)
            {
                if (detectedPot.GetPlantedPlant() != null)
                {
                    Plant p = detectedPot.GetPlantedPlant();

                    if (p.ExpectedGardenItem() == GardenItemType.Fertilizer)
                        detectedPot.outline.ChangeOutlineColor(Color.green, true);
                    
                    else
                        detectedPot.outline.ChangeOutlineColor(Color.red, true);
                }

                else
                    detectedPot.outline.ChangeOutlineColor(Color.white, false);
            }
        }

        else if (GetBelowFlowerPot() == null && detectedPot != null)
        {
            if (detectedPot.GetPlantedPlant() != null)
            {
                detectedPot.outline.ChangeOutlineColor(Color.white, false);
                detectedPot = null;
            }

            else if (detectedPot.GetPlantedPlant() != null)
            {
                detectedPot.outline.ChangeOutlineColor(new Color(252f / 256f, 157f / 256f, 3f / 256f), true);
                detectedPot = null;
            }
        }

        base.DetectEffect();
    }

    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (GetBelowFlowerPot() != null)
        {
            FlowerPot fp = GetBelowFlowerPot();

            if (fp.GetPlantedPlant() != null && fp.GetPlantedPlant().ExpectedGardenItem() == GardenItemType.Fertilizer && fp.canApplyItem)
            {
                fp.canApplyItem = false;
                animator.SetTrigger("fertilizer");
                GardenItemSFX();
                fp.GetPlantedPlant().ApplyGardenItem(GardenItemType.Fertilizer);
                fp.outline.ChangeOutlineColor(Color.red, false);
                SeedDatabase.instance.GardenUse(GardenItemType.Fertilizer, false);
                
                foreach (GardenItem gi in SeedDatabase.instance.fertilizerUI.items)
                {
                    gi.CheckForUsability();
                    SeedDatabase.instance.SendGardenDataToCollector();
                }
            }
        }

        base.GardenItemAction(ctx);
    }
}
