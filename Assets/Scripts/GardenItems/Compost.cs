using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Compost : GardenItem
{
    [SerializeField] private float compostEffectDuration;
    [SerializeField] private Sprite sittingSprite, grabbedSprite;

    private SpriteRenderer rend;

    public override void Awake()
    {
        rend = transform.GetChild(0).GetComponent<SpriteRenderer>();

        base.Awake();
    }

    public override void Update()
    {
        if (grabbed) rend.sprite = grabbedSprite;
        else rend.sprite = sittingSprite;

        base.Update();
    }

    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (GetBelowFlowerPot() != null)
        {   
            FlowerPot fp = GetBelowFlowerPot();

            if (fp.GetPlantedPlant() != null)
            {
                StartCoroutine(fp.TriggerCompostEffect(compostEffectDuration));
                fp.GetPlantedPlant().ApplyGardenItem(GardenItemType.Compost);
            }
        }
        
        base.GardenItemAction(ctx);
    }
}
