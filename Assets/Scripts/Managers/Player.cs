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
    public Plant holdingPlant, hoveringPlantedPlant, holdingPlantedPlant, removingPlant;
    public FlowerPot hoveringFlowerPot, holdingFlowerPot, placingFlowerPot, holdingPlacedFlower, removingFlower;
    public Pinata holdingPinata;
    private bool grabbedPlantedPlant, grabbedPlacedFlower;
    private GardenItem holdingGardenItem;
    private SeedDatabase seedDatabase;
    public bool onSellingPlantPanel, onUpgradingPlantPanel, onDecoPlantPanel;
    [Header("Economy")]
    [SerializeField] private int currentPlayerCoins;

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
        //Left Hand
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

        //Right Hand
        rightHand.grabbedItemEffect.Enable();
        rightHand.grabbedItemEffect = xrDirect.XRIRightHandInteraction.Activate;
        rightHand.grabbedItemEffect.performed += GrabPlantedPlant;
        rightHand.grabbedItemEffect.performed += RePlantGrabbedPlant;
        rightHand.grabbedItemEffect.performed += DeleteFlowerPot;
        rightHand.grabbedItemEffect.performed += DeletePlant;
        rightHand.grabbedItemEffect.performed += SquishPinata;
        rightHand.grabbedItemEffect.performed += SellActionMenu;
        rightHand.grabbedItemEffect.performed += UpgradeActionMenu;

        rightHand.sellPlant.Enable();
        rightHand.sellPlant = xrDirect.XRIRightHandInteraction.ButtonA;
        rightHand.sellPlant.performed += TriggerSellActionMenu;
        rightHand.sellPlant.performed += SquishPinata;

        rightHand.upgradePlant.Enable();
        rightHand.upgradePlant = xrDirect.XRIRightHandInteraction.ButtonB;
        rightHand.upgradePlant.performed += TriggerUpgradeActionMenu;
    }

    public void GrabPlantedPlant(InputAction.CallbackContext ctx)
    {
        if (holdingPlantedPlant != null)
        {
            holdingPlantedPlant.SetHandPosition(rightHand.handInteractor.transform);
            holdingPlantedPlant.TriggerReplant();
        }
    }

    public void SpendMoney(int amount)
    {
        if (currentPlayerCoins > 0)
            currentPlayerCoins -= amount;

        DataCollector.instance.SetPlayerCoins(currentPlayerCoins);
    }

    public void SetMoneyAmount(int amount)
    {
        currentPlayerCoins = amount;
    }

    public bool CanSpendMoney(int amount)
    {
        return GetPlayerMoney() >= amount;
    }

    public void AddMoney(int amount)
    {
        if (currentPlayerCoins < 99999999)
            currentPlayerCoins += amount;

        DataCollector.instance.SetPlayerCoins(currentPlayerCoins);
    }

    public int GetPlayerMoney()
    {
        return currentPlayerCoins;
    }

    public void PlantSelectedPlant(InputAction.CallbackContext ctx)
    {
        if ((holdingPlant != null && hoveringFlowerPot != null) && seedDatabase.CanPlant(holdingPlant.plantData))
        {
            if (hoveringFlowerPot.GetPlantedPlant() == null && hoveringFlowerPot.GetIfPlantIsAccepted())
            {
                hoveringFlowerPot.PlantPlant(holdingPlant);
                hoveringFlowerPot.outline.ChangeOutlineColor(Color.white, false);
                holdingPlant = null;
                hoveringFlowerPot = null;
            }

            else
                StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("cantselect"));
        }
    }

    public void RePlantGrabbedPlant(InputAction.CallbackContext ctx)
    {
        if (holdingPlantedPlant != null && hoveringFlowerPot != null)
        {
            if (hoveringFlowerPot.GetIfPlantIsAccepted())
            {
                DataCollector.instance.ReplacePlantInFlowerPot(GameManager.instance.GetGameWorldFromString(hoveringFlowerPot.createdIn),
                    holdingPlantedPlant, holdingPlantedPlant.flowerPotIn, hoveringFlowerPot);
                hoveringFlowerPot.RePlantPlant(holdingPlantedPlant);
                holdingPlantedPlant = null;
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

    public void RecievePlantedPlant(Plant p)
    {
        if (!grabbedPlantedPlant && p != null)
        {
            holdingPlantedPlant = p;
            grabbedPlantedPlant = true;
        }

        else if (p == null)
        {
            holdingPlantedPlant = null;
            grabbedPlantedPlant = false;
        }
    }

    public void SafetyNetsWhenHolding()
    {
        if (holdingPlant != null)
        {
            DestroyHoldingPlant();
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("tap"));
        }

        if (placingFlowerPot != null)
        {
            DestroyHoldingFlowerPot();
            SeedDatabase.instance.TriggerHolders(false);
            StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("tap"));
        }
    }

    public void RightHandEnabler(bool en)
    {
        rightHand.handInteractor.enabled = en;
    }

    public void LeftHandEnabler(bool en)
    {
        leftHand.handInteractor.enabled = en;
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

    public void DeletePlant(InputAction.CallbackContext ctx)
    {
        if (removingPlant != null)
            removingPlant.RemovePlant();
    }

    public void DeleteFlowerPot(InputAction.CallbackContext ctx)
    {
        if (removingFlower != null)
            removingFlower.RemoveFlowerPot();
    }

    public void RecieveToDeleteFlowerPot(FlowerPot fp)
    {
        removingFlower = fp;
    }

    public void RecieveToDeletePlant(Plant p)
    {
        removingPlant = p;
    }

    public void SquishPinata(InputAction.CallbackContext ctx)
    {
        if (holdingPinata != null)
            holdingPinata.Squish();
    }

    public void DestroyHoldingPlant()
    {
        if (holdingPlant.plantIsAbove != null)
            holdingPlant.plantIsAbove.outline.ChangeOutlineColor(Color.white, false);

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

    public void SellActionMenu(InputAction.CallbackContext ctx)
    {
        if (onSellingPlantPanel)
        {
            Player.instance.AddMoney(holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex
            ].sellPrice);
            DeTriggerPanels();
            holdingFlowerPot.GetPlantedPlant().TriggerSellPlant();
            holdingFlowerPot.ToggleFlowerPotUI(false);
            holdingFlowerPot.potInteractable.enabled = false;
            SoundEffectsManager.instance.PlaySoundEffectNC("money");
        }
    }

    public void UpgradeActionMenu(InputAction.CallbackContext ctx)
    {
        if (onDecoPlantPanel)
        {
            holdingFlowerPot.GetPlantedPlant().SetPlantAsDeco();
            SoundEffectsManager.instance.PlaySoundEffectNC("prize");
            holdingFlowerPot.ToggleFlowerPotUI(false);
            DeTriggerPanels();
            holdingFlowerPot.potInteractable.enabled = false;
            holdingFlowerPot.upgradePlantPanel.SetActive(false);
        }

        if (onUpgradingPlantPanel)
        {
            if (Player.instance.CanSpendMoney(
                holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex + 1
                ].upgradePrice))
            {
                SoundEffectsManager.instance.PlaySoundEffectNC("money");
                Player.instance.SpendMoney(holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex + 1
                ].upgradePrice);
                holdingFlowerPot.GetPlantedPlant().UpgradePlant();
                DeTriggerPanels();
                holdingFlowerPot.potInteractable.enabled = false;
                holdingFlowerPot.upgradePlantPanel.SetActive(false);
            }

            else SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
        }
    }

    public void TriggerSellActionMenu(InputAction.CallbackContext ctx)
    {
        if (holdingFlowerPot != null && !holdingFlowerPot.GetPlantedPlant().isDeco && !onUpgradingPlantPanel && !onSellingPlantPanel && !holdingFlowerPot.GetPlantedPlant().ExpectingGardenItem())
        {
            holdingFlowerPot.sellPlantPanel.SetActive(true);
            holdingFlowerPot.actionButtons.SetActive(false);
            holdingFlowerPot.plantSellT.text = "$ " + holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex
            ].sellPrice.ToString("N0");
            holdingFlowerPot.ToggleFlowerPotUI(false);
            onSellingPlantPanel = true;
        }

        else if (holdingFlowerPot != null && !onUpgradingPlantPanel && !onSellingPlantPanel && holdingFlowerPot.GetPlantedPlant().ExpectingGardenItem())
            SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
    }

    public void TriggerUpgradeActionMenu(InputAction.CallbackContext ctx)
    {
        if (holdingFlowerPot != null && !holdingFlowerPot.GetPlantedPlant().isDeco && !onSellingPlantPanel && !onUpgradingPlantPanel && !holdingFlowerPot.GetPlantedPlant().ExpectingGardenItem())
        {
            holdingFlowerPot.actionButtons.SetActive(false);
            holdingFlowerPot.ToggleFlowerPotUI(false);
            holdingFlowerPot.upgradePlantPanel.SetActive(true);

            if (holdingFlowerPot.GetPlantedPlant().CanUpgradePlant())
            {
                holdingFlowerPot.normalUPlantPanel.SetActive(true);

                holdingFlowerPot.plantUpgradeT.text = "$ " + holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex + 1
                ].upgradePrice.ToString("N0");

                holdingFlowerPot.upgradeProdTime.text = holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex
                ].producingTime + " <color=orange> >>> </color> <color=green>" + holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex + 1
                ].producingTime + "</color>";

                holdingFlowerPot.upgradeCoins.text = holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex
                ].producedCoins + " <color=orange> >>> </color> <color=green>" + holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex + 1
                ].producedCoins + "</color>";

                holdingFlowerPot.upgradeEnergy.text = holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex
                ].energyLevel + " <color=orange> >>> </color> <color=green>" + holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex + 1
                ].energyLevel + "</color>";

                holdingFlowerPot.upgradeLifeTime.text = holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex
                ].plantLife + " <color=orange> >>> </color> <color=green>" + holdingFlowerPot.GetPlantedPlant().plantData.plantLevels[
                    holdingFlowerPot.GetPlantedPlant().currentPlantLevelIndex + 1
                ].plantLife + "</color>";

                onUpgradingPlantPanel = true;
            }

            else
            {
                holdingFlowerPot.decoPlantPanel.SetActive(true);
                onDecoPlantPanel = true;
            }
        }

        else if (holdingFlowerPot != null && !holdingFlowerPot.GetPlantedPlant().isDeco && !onSellingPlantPanel && !onUpgradingPlantPanel && holdingFlowerPot.GetPlantedPlant().ExpectingGardenItem())
            SoundEffectsManager.instance.PlaySoundEffectNC("cantselect");
    }

    public void DeTriggerPanels()
    {
        holdingFlowerPot.upgradePlantPanel.SetActive(false);
        holdingFlowerPot.decoPlantPanel.SetActive(false);
        holdingFlowerPot.normalUPlantPanel.SetActive(false);
        holdingFlowerPot.sellPlantPanel.SetActive(false);

        if (holdingFlowerPot.GetPlantedPlant() != null)
            holdingFlowerPot.ToggleFlowerPotUI(!holdingFlowerPot.GetPlantedPlant().isDeco);

        onUpgradingPlantPanel = false;
        onSellingPlantPanel = false;
        onDecoPlantPanel = false;
    }

    public void SetFlowerPot(InputAction.CallbackContext ctx)
    {
        if (placingFlowerPot != null && placingFlowerPot.hoveringHolder != null)
        {
            DataCollector.instance.UpdateFlowerPotData(placingFlowerPot.flowerPotAsset.flowerPotType, SeedDatabase.instance.GetFlowerPotData(placingFlowerPot.flowerPotAsset).amount - 1);
            placingFlowerPot.transform.position = placingFlowerPot.hoveringHolder.transform.position;
            placingFlowerPot.transform.rotation = placingFlowerPot.hoveringHolder.transform.rotation;
            placingFlowerPot.startPos = placingFlowerPot.transform.position;
            placingFlowerPot.hoveringHolder.transform.GetChild(0).gameObject.SetActive(false);
            placingFlowerPot.hoveringHolder.GetComponent<BoxCollider>().enabled = false;
            placingFlowerPot.inPositionOfHolder = placingFlowerPot.hoveringHolder;
            placingFlowerPot.transform.parent = placingFlowerPot.hoveringHolder.transform;
            placingFlowerPot.createdIn = MusicManager.instance.currentWorld.ToString();
            DataCollector.instance.AddFlowerPot(MusicManager.instance.GetCurrentMusic().world, placingFlowerPot);
            placingFlowerPot.hoveringHolder = null;
            placingFlowerPot.setted = true;
            placingFlowerPot.triggerColl.enabled = true;
            seedDatabase.UseFlowerPot(placingFlowerPot.flowerPotAsset);
            seedDatabase.TriggerHolders(false);
            SoundEffectsManager.instance.PlaySoundEffectNC("ceramic");
            placingFlowerPot = null;
        }
    }

    public void RecievePinata(Pinata pinata = null)
    {
        holdingPinata = pinata;
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

    public void TogglePlayerHands(bool toggle)
    {
        leftHand.handInteractor.gameObject.SetActive(false);
        rightHand.handInteractor.gameObject.GetComponent<Collider>().enabled = toggle;
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
    public InputAction upgradePlant;
}
