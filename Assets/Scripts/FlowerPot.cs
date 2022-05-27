using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPot : MonoBehaviour
{
    [SerializeField] Transform plantPlantingPosition;

    private Plant hoveringPlant;
    Collider triggerCollider;

    void Start()
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            if (c.isTrigger)
            {
                triggerCollider = c;
                break;
            }
        }
    }

    public void PlantPlant(Plant p)
    {
        p.transform.position = plantPlantingPosition.position;
        p.transform.parent = transform;
        p.ApplyColorToPlant();
        p.SetPlanted();
        triggerCollider.enabled = false;
    }
}
