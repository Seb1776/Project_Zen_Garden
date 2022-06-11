using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    private XRIDefaultInputActions xrDirect;
    public VRHandsLeft leftHand;
    public VRHandsRight rightHand;
    private bool oneTeleporterEnabled;
    private Plant holdingPlant;
    private FlowerPot hoveringFlowerPot, holdingFlowerPot;
    private GardenItem holdingGardenItem;
    private SeedDatabase seedDatabase;
    [Header ("Economy")]
    public int currentPlayerCoins;

    void Awake()
    {
        seedDatabase = GameObject.FindGameObjectWithTag("SeedDatabase").GetComponent<SeedDatabase>();

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
        leftHand.move = xrDirect.XRILeftHandLocomotion.Move;
        leftHand.move.performed += TeleportEnabler;

        leftHand.plantAction.Enable();
        leftHand.plantAction = xrDirect.XRILeftHandInteraction.Activate;
        leftHand.plantAction.performed += PlantSelectedPlant;

        leftHand.removePlant.Enable();
        leftHand.removePlant = xrDirect.XRILeftHandInteraction.ButtonA;
        leftHand.removePlant.performed += GetRidOfSelectedPlant;

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
        }
    }

    public void SetHoldFlowerPot(FlowerPot pot = null)
    {
        holdingFlowerPot = pot;
    }

    public void GetRidOfSelectedPlant(InputAction.CallbackContext ctx)
    {
        if (holdingPlant != null)
            DestroyHoldingPlant();
    }

    public void DestroyHoldingPlant()
    {
        Destroy(holdingPlant.gameObject);
        holdingPlant = null;
        hoveringFlowerPot = null;
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

    public GardenItem GetHoldingGardenItem()
    {
        return holdingGardenItem;
    }

    public void CreatedAPlant(Plant _p)
    {
        holdingPlant = _p;
    }

    public Plant GetHoldingPlant()
    {
        return holdingPlant;
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
            if (ctx.ReadValue<Vector2>().magnitude > 0.1)
            {   
                if (!oneTeleporterEnabled)
                    oneTeleporterEnabled = true;
            }

            else
                oneTeleporterEnabled = false;
            
            leftHand.handInteractor.enabled = oneTeleporterEnabled;
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
