using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPot : MonoBehaviour
{
    [SerializeField] Transform plantPlantingPosition;
    
    PlantsManager plantsManager;
    private Plant hoveringPlant;
    Collider triggerCollider;

    void Awake()
    {
        plantsManager = GameObject.FindGameObjectWithTag("PlantsManager").GetComponent<PlantsManager>();
    }

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
        GameObject _s = Instantiate(plantsManager.GetSprout(), plantPlantingPosition.position, Quaternion.identity);
        _s.transform.position = plantPlantingPosition.position;
        p.transform.position = plantPlantingPosition.position;
        p.transform.parent = transform;
        _s.transform.parent = transform;
        p.ApplyColorToPlant();
        p.SetPlanted(_s);
        triggerCollider.enabled = false;
    }
}
