using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    private XRIDefaultInputActions xrDirect;
    public VRHands leftHand;
    private bool oneTeleporterEnabled;
    private Plant holdingPlant;
    private FlowerPot hoveringFlowerPot;

    void Awake()
    {
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
    }

    public void PlantSelectedPlant(InputAction.CallbackContext ctx)
    {
        if (holdingPlant != null && hoveringFlowerPot != null)
        {
            hoveringFlowerPot.PlantPlant(holdingPlant);
            holdingPlant = null;
            hoveringFlowerPot = null;
        }
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
public class VRHands
{
    public XRRayInteractor handInteractor;
    public InputAction move;
    public InputAction plantAction;
    public InputAction removePlant;
    public bool canUse = true;
}
