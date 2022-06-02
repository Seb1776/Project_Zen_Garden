using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPot : MonoBehaviour
{
    [SerializeField] private Transform plantPlantingPosition;
    [SerializeField] private ParticleSystem waterEffect, compostEffect;
    [SerializeField] private GameObject musicPlayer;

    [Header ("UI")]
    [SerializeField] private GameObject needWaterIcon;
    [SerializeField] private GameObject needCompostIcon, needFertilizerIcon, needMusicIcon;
    
    private PlantsManager plantsManager;
    private Plant plantInSpace;

    void Awake()
    {
        plantsManager = GameObject.FindGameObjectWithTag("PlantsManager").GetComponent<PlantsManager>();
    }

    void Start()
    {

    }

    public Plant GetPlantedPlant()
    {
        return plantInSpace;
    }

    public void ActivateWarning(GardenItemType gip, bool activate)
    {
        switch (gip)
        {
            case GardenItemType.Water:
                needWaterIcon.SetActive(activate);
            break;

            case GardenItemType.Compost:
                needCompostIcon.SetActive(activate);
            break;

            case GardenItemType.Fertilizer:
                needFertilizerIcon.SetActive(activate);
            break;

            case GardenItemType.Music:
                needMusicIcon.SetActive(activate);
            break;
        }
    }

    public IEnumerator TriggerWaterEffect(float duration)
    {
        waterEffect.Play();
        yield return new WaitForSeconds(duration);
        waterEffect.Stop();
    }

    public IEnumerator TriggerCompostEffect(float duration)
    {
        compostEffect.Play();
        yield return new WaitForSeconds(duration);
        compostEffect.Stop();
    }

    public IEnumerator TriggerMusicEffect(float duration)
    {
        musicPlayer.SetActive(true);
        yield return new WaitForSeconds(duration);
        musicPlayer.SetActive(false);
    }

    public void PlantPlant(Plant p)
    {
        GameObject _s = Instantiate(plantsManager.GetSprout(), plantPlantingPosition.position, Quaternion.identity);
        _s.transform.position = plantPlantingPosition.position;
        p.transform.position = plantPlantingPosition.position;
        p.transform.parent = transform;
        p.flowerPotIn = this;
        _s.transform.parent = transform;
        plantInSpace = p;
        p.ApplyColorToPlant();
        p.SetPlanted(_s);
    }
}
