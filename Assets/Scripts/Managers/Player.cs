using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    public static Player instance;

    private XRIDefaultInputActions xrDirect;
    public VRHandsLeft leftHand;
    public VRHandsRight rightHand;
    private bool oneTeleporterEnabled;
    public Plant holdingPlant, hoveringPlantedPlant, holdingPlantedPlant;
    public FlowerPot hoveringFlowerPot, holdingFlowerPot, placingFlowerPot;
    private GardenItem holdingGardenItem;
    private SeedDatabase seedDatabase;
    [Header ("Economy")]
    public int currentPlayerCoins;

    void Awake()
    {
        seedDatabase = GameObject.FindGameObjectWithTag("SeedDatabase").GetComponent<SeedDatabase>();

        if (instance != null && instance != this) Destroy(this);
        else instance = this;

        xrDirect = new XRIDefaultInputActions();
        xrDirect.Enable();
    }

    void Start()
    {
        SetInteractions();
    }

    void SetInteractions()
    {
        leftHand.handInteractor.enabled = false;
        leftHand.move.Enable();
        leftHand.move = xrDirect.XRILeftHandInteraction.ButtonA;
        leftHand.move.performed += TeleportEnabler;
        leftHand.move.canceled += TeleportDisabler;

        leftHand.plantAction.Enable();
        leftHand.plantAction = xrDirect.XRILeftHandInteraction.Activate;
        leftHand.plantAction.performed += PlantSelectedPlant;
        leftHand.plantAction.performed += SetFlowerPot;

        leftHand.removePlant.Enable();
        leftHand.removePlant = xrDirect.XRILeftHandInteraction.ButtonB;
        leftHand.removePlant.performed += GetRidOfSelectedPlant;
        leftHand.removePlant.performed += GetRidOfSelectedFlowerPot;

        rightHand.grabbedItemEffect.Enable();
        rightHand.grabbedItemEffect = xrDirect.XRIRightHandInteraction.Activate;

        rightHand.sellPlant.Enable();
        rightHand.sellPlant = xrDirect.XRIRightHandInteraction.ButtonA;
        rightHand.sellPlant.performed += SellPlant;
    }

    public void PlantSelectedPlant(InputAction.CallbackContext ctx)
    {
        if ((holdingPlant != null && hoveringFlowerPot != null) && seedDatabase.CanPlant(holdingPlant.plantData))
        {   
            if (hoveringFlowerPot.GetPlantedPlant() == null && hoveringFlowerPot.GetIfPlantIsAccepted())
            {
                hoveringFlowerPot.PlantPlant(holdingPlant);
                holdingPlant = null;
                hoveringFlowerPot = null;
            }

            else
                StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("cantselect"));
        }
    }

    public void SetHoldFlowerPot(FlowerPot pot = null)
    {
        holdingFlowerPot = pot;
    }

    public void GetRidOfSelectedPlant(InputAction.CallbackContext ctx)
    {
        if (holdingPlant != null)
        {
            DestroyHoldingPlant();
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("tap"));
        }
    }

    public void GetRidOfSelectedFlowerPot(InputAction.CallbackContext ctx)
    {
        if (placingFlowerPot != null)
        {
            DestroyHoldingFlowerPot();
            SeedDatabase.instance.TriggerHolders(false);
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("tap"));
        }
    }

    public void DestroyHoldingPlant()
    {
        Destroy(holdingPlant.gameObject);
        holdingPlant = null;
        hoveringFlowerPot = null;
    }

    public void DestroyHoldingFlowerPot()
    {
        Destroy(placingFlowerPot.gameObject);
        placingFlowerPot = null;
    }

    public void SetGardenItem(GardenItem garden)
    {   
        if (holdingGardenItem == null)
        {
            holdingGardenItem = garden;
            rightHand.grabbedItemEffect.performed += holdingGardenItem.GardenItemAction;
        }
    }

    public void DeSetGardenItem(GardenItem item)
    {   
        if (holdingGardenItem != null && holdingGardenItem == item)
        {
            rightHand.grabbedItemEffect.performed -= holdingGardenItem.GardenItemAction;
            holdingGardenItem = null;
        }
    }

    public void SellPlant(InputAction.CallbackContext ctx)
    {
        if (holdingFlowerPot != null)
        {
            GameObject plant;
            plant = holdingFlowerPot.GetPlantedPlant().gameObject;
            currentPlayerCoins += plant.GetComponent<Plant>().plantData.revenuePrice;
            plant.GetComponent<Plant>().TriggerSellPlant();
        }
    }

    public void SetFlowerPot(InputAction.CallbackContext ctx)
    {
        if (placingFlowerPot != null && placingFlowerPot.hoveringHolder != null)
        {
            placingFlowerPot.transform.position = placingFlowerPot.hoveringHolder.transform.position;
            placingFlowerPot.transform.rotation = placingFlowerPot.hoveringHolder.transform.rotation;
            placingFlowerPot.startPos = placingFlowerPot.transform.position;
            placingFlowerPot.hoveringHolder.gameObject.SetActive(false);
            placingFlowerPot.hoveringHolder = null;
            placingFlowerPot.setted = true;
            placingFlowerPot.triggerColl.enabled = true;
            seedDatabase.UseFlowerPot(placingFlowerPot.flowerPotAsset);
            placingFlowerPot = null;
        }
    }

    public GardenItem GetHoldingGardenItem()
    {
        return holdingGardenItem;
    }

    public void CreatedAPlant(Plant _p)
    {
        holdingPlant = _p;
    }

    public void CreatedAFlowerPot(FlowerPot _fp)
    {
        placingFlowerPot = _fp;
    }

    public Plant GetHoldingPlant()
    {
        return holdingPlant;
    }

    public FlowerPot GetHoldingFlowerPot()
    {
        return placingFlowerPot;
    }

    public void HoveringAFlowerPot(FlowerPot _fp)
    {
        hoveringFlowerPot = _fp;
    }

    public FlowerPot GetHoveringFlowerPot()
    {
        return hoveringFlowerPot;
    }

    void TeleportEnabler(InputAction.CallbackContext ctx)
    {   
        if (leftHand.canUse)
        {
            if (ctx.performed)
                oneTeleporterEnabled = true;
            
            leftHand.handInteractor.enabled = true;
        }
    }

    void TeleportDisabler(InputAction.CallbackContext ctx)
    {   
        if (leftHand.canUse)
        {
            if (ctx.performed)
                oneTeleporterEnabled = false;
            
            leftHand.handInteractor.enabled = false;
        }
    }
}

[System.Serializable]
public class VRHandsLeft
{
    public XRRayInteractor handInteractor;
    public InputAction move;
    public InputAction plantAction;
    public InputAction removePlant;
    public bool canUse = true;
}

[System.Serializable]
public class VRHandsRight
{
    public XRDirectInteractor handInteractor;
    public InputAction grabbedItemEffect;
    public InputAction sellPlant;
}
