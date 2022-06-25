using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Phonograph : GardenItem
{
    [SerializeField] private float phonographDuration;

    private FlowerPot hoveringFlowerPot;

    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (hoveringFlowerPot != null && hoveringFlowerPot.GetPlantedPlant() != null)
        {   
            if (hoveringFlowerPot.GetComponent<FlowerPot>().canApplyItem)
            {
                if (hoveringFlowerPot.GetPlantedPlant() != null && hoveringFlowerPot.GetPlantedPlant().ExpectedGardenItem() == GardenItemType.Music  && hoveringFlowerPot.canApplyItem)
                {
                    hoveringFlowerPot.GetComponent<FlowerPot>().canApplyItem = false;
                    StartCoroutine(hoveringFlowerPot.TriggerMusicEffect());
                    hoveringFlowerPot.GetPlantedPlant().ApplyGardenItem(GardenItemType.Music);
                }
            }
        }

        base.GardenItemAction(ctx);
    }

    void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("FlowerPot"))
        {   
            if (hoveringFlowerPot.GetPlantedPlant() != null)
            {
                if (!hoveringFlowerPot.GetPlantedPlant().fullyGrown)
                {
                    hoveringFlowerPot.outline.ChangeOutlineColor(Color.red, false);
                    hoveringFlowerPot = null;
                }

                else
                {
                    hoveringFlowerPot.outline.ChangeOutlineColor(new Color(252f / 256f, 157f / 256f, 3f / 256f), true);
                    hoveringFlowerPot = null;
                }
            }
        }
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("FlowerPot"))
        {
            hoveringFlowerPot = c.transform.GetComponent<FlowerPot>();

            if (hoveringFlowerPot.canApplyItem)
            {
                if (hoveringFlowerPot.GetPlantedPlant() != null)
                {
                    Plant p = hoveringFlowerPot.GetPlantedPlant();

                    if (p.ExpectedGardenItem() == GardenItemType.Music)
                        hoveringFlowerPot.outline.ChangeOutlineColor(Color.green, true);
                    
                    else
                        hoveringFlowerPot.outline.ChangeOutlineColor(Color.red, true);
                }
            }
        }
    }
}
