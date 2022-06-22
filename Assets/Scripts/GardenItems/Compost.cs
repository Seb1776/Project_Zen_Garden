using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Compost : GardenItem
{
    [SerializeField] private float compostEffectDuration;
    [SerializeField] private Vector2 acceptableRotationRange;
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
    
        if (transform.eulerAngles.z >= acceptableRotationRange.x && transform.eulerAngles.z <= acceptableRotationRange.y)
            canUseItem = true;
        
        else
            canUseItem = false;

        base.Update();
    }

    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (GetBelowFlowerPot() != null && canUseItem)
        {   
            FlowerPot fp = GetBelowFlowerPot();

            if (fp.GetPlantedPlant() != null && fp.GetPlantedPlant().ExpectedGardenItem() == GardenItemType.Compost && fp.canApplyItem)
            {
                StartCoroutine(fp.TriggerCompostEffect(compostEffectDuration));
                GardenItemSFX();
                fp.GetPlantedPlant().ApplyGardenItem(GardenItemType.Compost);
            }
        }
        
        base.GardenItemAction(ctx);
    }
}
