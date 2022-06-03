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
        if (hoveringFlowerPot != null)
        {
            if (hoveringFlowerPot.GetPlantedPlant() != null && hoveringFlowerPot.GetPlantedPlant().ExpectedGardenItem() == GardenItemType.Music)
            {
                StartCoroutine(hoveringFlowerPot.TriggerMusicEffect(phonographDuration));
                hoveringFlowerPot.GetPlantedPlant().ApplyGardenItem(GardenItemType.Music);
            }
        }

        base.GardenItemAction(ctx);
    }

    void OnTriggerExit(Collider c)
    {
        if (c.CompareTag("FlowerPot"))
        {
            hoveringFlowerPot = null;
        }
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.CompareTag("FlowerPot"))
        {
            hoveringFlowerPot = c.transform.GetComponent<FlowerPot>();
        }
    }
}
