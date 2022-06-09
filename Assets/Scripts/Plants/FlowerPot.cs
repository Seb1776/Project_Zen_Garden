using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(AudioSource))]
public class FlowerPot : MonoBehaviour
{
    [SerializeField] private Transform plantPlantingPosition;
    [SerializeField] private ParticleSystem waterEffect, compostEffect;
    [SerializeField] private GameObject musicPlayer;
    [SerializeField] private GameObject sellPlantButton;

    [Header ("UI")]
    [SerializeField] private GameObject needWaterIcon;
    [SerializeField] private GameObject needCompostIcon, needFertilizerIcon, needMusicIcon;
    [SerializeField] private Text revenueText;

    [Header ("Effect")]
    [SerializeField] private ParticleSystem grownEffect;
    
    private Player player;
    private Collider coll;
    private PlantsManager plantsManager;
    private Plant plantInSpace;
    private SeedDatabase seedDatabase;
    private AudioSource source;
    private MusicManager musicManager;
    private XRGrabInteractable potInteractable;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        potInteractable = GetComponent<XRGrabInteractable>();
        coll = GetComponent<Collider>();
        
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        musicManager = GameObject.FindGameObjectWithTag("MusicManager").GetComponent<MusicManager>();
        plantsManager = GameObject.FindGameObjectWithTag("PlantsManager").GetComponent<PlantsManager>();
        seedDatabase = GameObject.FindGameObjectWithTag("SeedDatabase").GetComponent<SeedDatabase>();
    }

    [System.Obsolete]
    void Start()
    {
        potInteractable.onSelectEnter.AddListener(SendFlowerPotToPlayer);
        potInteractable.onSelectExit.AddListener(UnSendFlowerPotToPlayer);
    }

    void Update()
    {
        coll.enabled = !potInteractable.isSelected;
    }

    void SendFlowerPotToPlayer(XRBaseInteractor flower)
    {
        player.SetHoldFlowerPot(this);

        if (plantInSpace != null)
        {
            sellPlantButton.SetActive(true);
            revenueText.text = "Sell For: $ " + plantInSpace.GetActualRevenue().ToString();
            revenueText.gameObject.SetActive(true);
        }
    }

    void UnSendFlowerPotToPlayer(XRBaseInteractor flower)
    {
        player.SetHoldFlowerPot();
        sellPlantButton.SetActive(false);
        revenueText.gameObject.SetActive(false);
    }
    
    public void TriggerPlantSell()
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

    public void PlayFullGrown()
    {
        grownEffect.gameObject.SetActive(true);
        source.PlayOneShot(musicManager.GetCurrentMusic().grownPlantClip);
    }

    public IEnumerator TriggerMusicEffect()
    {
        musicPlayer.SetActive(true);
        source.PlayOneShot(musicManager.GetCurrentMusic().phonographClip);
        yield return new WaitForSeconds(musicManager.GetCurrentMusic().phonographClip.length);
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
        seedDatabase.UsePlant(p.plantData);
        plantInSpace = p;
        p.ApplyColorToPlant();
        p.SetPlanted(_s);
    }
}
