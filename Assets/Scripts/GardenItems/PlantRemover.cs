using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlantRemover : GardenItem
{
    private XRGrabInteractable moverInteractable;
    private Plant detectedPlant;
    private FlowerPot detectedFlowerPot;

    [System.Obsolete]
    public override void Start()
    {
        moverInteractable = GetComponent<XRGrabInteractable>();

        moverInteractable.onSelectEnter.AddListener(PlayShovel);
        moverInteractable.onSelectExit.AddListener(DeselectDetected);

        base.Start();
    }

    public override void DetectEffect()
    {
        if (detectedFlowerPot != null)
            detectedFlowerPot.outline.ChangeOutlineColor(Color.gray, true);
        
        else
            detectedFlowerPot.outline.ChangeOutlineColor(Color.gray, false);
        
        if (detectedPlant != null)
            detectedPlant.flowerPotIn.outline.ChangeOutlineColor(Color.gray, true);
        
        else
            detectedPlant.flowerPotIn.outline.ChangeOutlineColor(Color.gray, false);

        base.DetectEffect();
    }

    void PlayShovel(XRBaseInteractor i)
    {
        SoundEffectsManager.instance.PlaySoundEffectNC("shovel");
    }

    void DeselectDetected(XRBaseInteractor i)
    {
        if (detectedFlowerPot != null)
        {
            detectedFlowerPot.outline.ChangeOutlineColor(Color.gray, false);
            detectedFlowerPot = null;
            Player.instance.RecieveToDeleteFlowerPot(null);
        }
        
        if (detectedPlant != null)
        {
            detectedPlant.flowerPotIn.outline.ChangeOutlineColor(Color.gray, false);
            detectedPlant = null;
            Player.instance.RecieveToDeletePlant(null);
        }
    }

    void OnTriggerStay(Collider other)
    {   
        if (moverInteractable.isSelected)
        {        
            if (detectedPlant == null && detectedFlowerPot == null)
            {
                if (other.CompareTag("Plant"))
                {
                    detectedPlant = other.transform.GetComponent<Plant>();
                    detectedPlant.flowerPotIn.selectedByShovel = true;
                    Player.instance.RecieveToDeletePlant(detectedPlant);
                    detectedPlant.flowerPotIn.outline.ChangeOutlineColor(Color.black, true);
                }

                else if (other.CompareTag("FlowerPot"))
                {
                    detectedFlowerPot = other.transform.GetComponent<FlowerPot>();
                    detectedFlowerPot.selectedByShovel = true;
                    Player.instance.RecieveToDeleteFlowerPot(detectedFlowerPot);
                    detectedFlowerPot.outline.ChangeOutlineColor(Color.gray, true);
                }
            }

            else if (detectedPlant != null)
            {
                if (other.CompareTag("FlowerPot"))
                {
                    detectedPlant = null;
                    detectedFlowerPot = other.transform.GetComponent<FlowerPot>();
                    detectedFlowerPot.selectedByShovel = true;
                    detectedFlowerPot.outline.ChangeOutlineColor(Color.gray, true);
                    Player.instance.RecieveToDeletePlant(detectedPlant);
                    Player.instance.RecieveToDeleteFlowerPot(detectedFlowerPot);
                }
            }

            else if (detectedFlowerPot != null)
            {
                if (other.CompareTag("Plant"))
                {
                    detectedFlowerPot = null;
                    detectedPlant = other.transform.GetComponent<Plant>();
                    detectedPlant.flowerPotIn.selectedByShovel = true;
                    detectedPlant.flowerPotIn.outline.ChangeOutlineColor(Color.black, true);
                    Player.instance.RecieveToDeletePlant(detectedPlant);
                    Player.instance.RecieveToDeleteFlowerPot(detectedFlowerPot);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {   
        if (detectedPlant != null && other.CompareTag("Plant"))
        {
            detectedPlant.flowerPotIn.selectedByShovel = false;
            
            if (!detectedFlowerPot.GetPlantedPlant().isDeco)
                detectedFlowerPot.outline.ChangeOutlineColor(Color.gray, false);
            
            else
                detectedFlowerPot.outline.ChangeOutlineColor(new Color32(250, 114, 2, 255), true);

            detectedPlant = null;
            Player.instance.RecieveToDeletePlant(null);
        }

        else if (detectedFlowerPot != null && other.CompareTag("FlowerPot"))
        {
            detectedFlowerPot.selectedByShovel = false;

            if (!detectedFlowerPot.GetPlantedPlant().isDeco)
                detectedFlowerPot.outline.ChangeOutlineColor(Color.gray, false);
            
            else
                detectedFlowerPot.outline.ChangeOutlineColor(new Color32(250, 114, 2, 255), true);

            detectedFlowerPot = null;
            Player.instance.RecieveToDeleteFlowerPot(null);
        }
    }
}
