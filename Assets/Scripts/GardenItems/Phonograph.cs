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
                    SeedDatabase.instance.GardenUse(GardenItemType.Music, false);
                    
                    foreach (GardenItem gi in SeedDatabase.instance.phonographUI.items)
                    {
                        gi.CheckForUsability();
                        SeedDatabase.instance.SendGardenDataToCollector();
                    }
                }
            }
        }

        base.GardenItemAction(ctx);
    }

    void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("FlowerPot"))
        {
            if (hoveringFlowerPot != null)
            {
                if (hoveringFlowerPot.GetPlantedPlant() != null)
                {
                    if (hoveringFlowerPot.GetPlantedPlant() != null)
                    {
                        hoveringFlowerPot.outline.ChangeOutlineColor(Color.red, false);
                        hoveringFlowerPot = null;
                    }

                    else if (hoveringFlowerPot.GetPlantedPlant() != null)
                    {
                        hoveringFlowerPot.outline.ChangeOutlineColor(new Color(252f / 256f, 157f / 256f, 3f / 256f), true);
                        hoveringFlowerPot = null;
                    }
                }

                else
                    hoveringFlowerPot = null;
            }
        }
    }

    public override void CheckForUsability()
    {
        if (!SeedDatabase.instance.GardenIsAvailable(GardenItemType.Music))
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

    void PhonographData(FlowerPot fp)
    {
        if (fp.canApplyItem)
        {
            if (fp.GetPlantedPlant() != null)
            {
                Plant p = fp.GetPlantedPlant();

                if (p.ExpectedGardenItem() == GardenItemType.Music)
                    hoveringFlowerPot.outline.ChangeOutlineColor(Color.green, true);
                
                else
                    hoveringFlowerPot.outline.ChangeOutlineColor(Color.red, true);
            }
        }
    }

    void OnTriggerStay(Collider c)
    {
        if (c.CompareTag("FlowerPot"))
        {   
            if (hoveringFlowerPot == null)
            {
                Debug.Log(c.transform.GetComponent<FlowerPot>());
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
}
