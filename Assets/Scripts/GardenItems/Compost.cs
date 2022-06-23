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

                    if (p.ExpectedGardenItem() == GardenItemType.Compost)
                        detectedPot.outline.ChangeOutlineColor(Color.green, true);
                    
                    else
                        detectedPot.outline.ChangeOutlineColor(Color.red, true);
                }
            }
        }

        else if (GetBelowFlowerPot() == null && detectedPot != null)
        {
            if (!detectedPot.GetPlantedPlant().fullyGrown)
            {
                detectedPot.outline.ChangeOutlineColor(Color.white, false);
                detectedPot = null;
            }

            else
            {
                detectedPot.outline.ChangeOutlineColor(new Color(252f / 256f, 157f / 256f, 3f / 256f), true);
                detectedPot = null;
            }
        }

        base.DetectEffect();
    }

    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (GetBelowFlowerPot() != null && canUseItem)
        {   
            FlowerPot fp = GetBelowFlowerPot();

            if (fp.canApplyItem)
            {
                if (fp.GetPlantedPlant() != null && fp.GetPlantedPlant().ExpectedGardenItem() == GardenItemType.Compost && fp.canApplyItem)
                {
                    fp.canApplyItem = false;
                    StartCoroutine(fp.TriggerCompostEffect(compostEffectDuration));
                    GardenItemSFX();
                    fp.GetPlantedPlant().ApplyGardenItem(GardenItemType.Compost);
                }
            }
        }
        
        base.GardenItemAction(ctx);
    }
}
