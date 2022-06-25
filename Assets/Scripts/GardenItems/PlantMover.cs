using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PlantMover : MonoBehaviour
{
    private XRGrabInteractable plantInteractable;

    void SendHoveredPlantedPlant(XRBaseInteractor interactor)
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Plant"))
        {
            Debug.Log(other.gameObject.name);
        }
    }
}
