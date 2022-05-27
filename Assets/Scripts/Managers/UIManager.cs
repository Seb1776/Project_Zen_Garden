using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Player player;

    public void SpawnPlantOnHand(Plant plant)
    {
        if (player.GetHoldingPlant() == null)
            CreatePlantOnPlayerHand(plant.gameObject);
        
        else
        {
            if (player.GetHoldingPlant().gameObject.name != plant.gameObject.name)
            {
                player.DestroyHoldingPlant();
                CreatePlantOnPlayerHand(plant.gameObject);
            }
        }
    }

    void CreatePlantOnPlayerHand(GameObject plant)
    {
        GameObject selectPlant = Instantiate(plant, 
            player.leftHand.handInteractor.transform.position, Quaternion.identity
        );

        selectPlant.gameObject.name = selectPlant.gameObject.name.Replace("(Clone)", string.Empty);
        selectPlant.GetComponent<Plant>().SetHandPosition(player.leftHand.handInteractor.transform);
        player.CreatedAPlant(selectPlant.GetComponent<Plant>());
    }
}
