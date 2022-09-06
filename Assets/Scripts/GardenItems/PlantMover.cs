using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlantMover : GardenItem
{
    private XRGrabInteractable moverInteractable;
    private Plant grabbedPlant;

    [System.Obsolete]
    public override void Start()
    {
        moverInteractable = GetComponent<XRGrabInteractable>();

        moverInteractable.onSelectExit.AddListener(ReturnPlantToOriginalLocation);

        base.Start();
    }

    void ReturnPlantToOriginalLocation(XRBaseInteractor i)
    {
        if (grabbedPlant != null && grabbedPlant.replanting)
        {
            grabbedPlant.flowerPotIn.RePlantPlant(grabbedPlant);
            Player.instance.RecievePlantedPlant(null);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Plant") && other.GetComponent<Plant>().growth)
        {
            grabbedPlant.flowerPotIn.outline.ChangeOutlineColor(Color.yellow, false);
            grabbedPlant = null;
            Player.instance.RecievePlantedPlant(grabbedPlant);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Plant") && other.GetComponent<Plant>().growth)
        {
            if (grabbedPlant == null)
            {
                grabbedPlant = other.transform.GetComponent<Plant>();
                grabbedPlant.flowerPotIn.outline.ChangeOutlineColor(Color.yellow, true);
                Player.instance.RecievePlantedPlant(grabbedPlant);
            }
        }
    }
}
