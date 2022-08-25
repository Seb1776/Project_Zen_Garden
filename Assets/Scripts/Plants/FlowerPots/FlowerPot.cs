using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public enum FlowerPotType
{
    Basket, Terracota, Jurassic, Holographic, FutureTech,
    Bronze, Silver, Golden, DarkEnergy, Crystal, Volcanic
}

[RequireComponent(typeof(AudioSource))]
public class FlowerPot : MonoBehaviour
{
    public FlowerPotAsset flowerPotAsset;
    public Transform plantPlantingPosition;
    [SerializeField] private ParticleSystem waterEffect, compostEffect;
    [SerializeField] private GameObject musicPlayer;
    public string createdIn;
    public OutlineEffect outline;
    public GameObject sellPlantButton;

    [Header ("UI")]
    [SerializeField] private GameObject needWaterIcon;
    [SerializeField] private GameObject needCompostIcon, needFertilizerIcon, needMusicIcon;
    public Text revenueText;

    [Header ("Effect")]
    [SerializeField] private ParticleSystem grownEffect;
    
    private Player player;
    private Collider coll;
    public Collider triggerColl;
    private PlantsManager plantsManager;
    [SerializeField] private Plant plantInSpace;
    private SeedDatabase seedDatabase;
    private AudioSource source;
    private MusicManager musicManager;
    private XRGrabInteractable potInteractable;
    private bool hoveringPlantIsAccepted, returnToPos, returning;
    public bool canUseOutline = true;
    public bool selectedByShovel;
    public bool canApplyItem;
    public bool setted, reAssignable;
    public Vector3 startPos;
    public FlowerPotHolder hoveringHolder, inPositionOfHolder;
    private Transform handPosition;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        potInteractable = GetComponent<XRGrabInteractable>();
        coll = GetComponent<Collider>();

        foreach (Collider c in GetComponents<Collider>())
            if (c.isTrigger)
                triggerColl = c;
        
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
        if (!setted)
            transform.position = handPosition.position;
        
        if (setted && !potInteractable.isSelected && MusicManager.instance.currentWorld.ToString() == createdIn && 
            Vector3.Distance(transform.position, startPos) > 0.01f)
            returnToPos = true;
        
        if (GetPlantedPlant() != null && (GetPlantedPlant().fullyGrown && !GetPlantedPlant().replanting) && !selectedByShovel)
            outline.ChangeOutlineColor(new Color(252f / 256f, 157f / 256f, 3f / 256f), true);
        
        if (returnToPos && MusicManager.instance.currentWorld.ToString() == createdIn)
            GetBackPos();
    }

    void GetBackPos()
    {
        transform.position = Vector3.Lerp(transform.position, startPos, 2.5f * Time.deltaTime);
        coll.isTrigger = true;
        triggerColl.enabled = false;
        returning = true;

        if (Vector3.Distance(transform.position, startPos) <= 0.001f)
        {
            transform.position = startPos;
            returnToPos = false;
            returning = false;
            coll.isTrigger = false;
            triggerColl.enabled = true;
        }
    }

    public void RemoveFlowerPot()
    {
        inPositionOfHolder.transform.GetChild(0).gameObject.SetActive(true);
        inPositionOfHolder.GetComponent<Collider>().enabled = true;

        SoundEffectsManager.instance.PlaySoundEffectNC("ceramic");
        
        if (plantInSpace != null)
            plantInSpace.RemovePlant();
        
        DataCollector.instance.RemoveFlowerPot(MusicManager.instance.GetCurrentMusic().world, this);

        Destroy(this.gameObject);
    }

    public void SetHandPosition(Transform hand)
    {
        handPosition = hand;
    }

    void SendFlowerPotToPlayer(XRBaseInteractor flower)
    {
        player.SetHoldFlowerPot(this);
        triggerColl.enabled = false;
        SeedDatabase.instance.TriggerHolders(true);

        if (plantInSpace != null)
        {
            sellPlantButton.SetActive(true);
            revenueText.gameObject.SetActive(true);
        }
    }

    public void UpdatePlantSellPrice(int newPrice)
    {
        revenueText.text = "Sell For: $ " + newPrice.ToString("0,0");
    }

    void UnSendFlowerPotToPlayer(XRBaseInteractor flower)
    {
        if (reAssignable)
        {
            startPos = hoveringHolder.transform.position;
            inPositionOfHolder.transform.GetChild(0).gameObject.SetActive(true);
            inPositionOfHolder.GetComponent<BoxCollider>().enabled = true;
            hoveringHolder.transform.GetChild(0).gameObject.SetActive(false);
            hoveringHolder.GetComponent<BoxCollider>().enabled = false;
            inPositionOfHolder = hoveringHolder;
            transform.rotation = inPositionOfHolder.transform.rotation;
            transform.parent = inPositionOfHolder.gardenParent;
            hoveringHolder = null;
            reAssignable = false;
        }

        triggerColl.enabled = plantInSpace == null;
        SeedDatabase.instance.TriggerHolders(false);
        player.SetHoldFlowerPot();
        sellPlantButton.SetActive(false);
        revenueText.gameObject.SetActive(false);
    }

    public FlowerPotHolder GetFPHolder()
    {
        return hoveringHolder;
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

        if (activate) canApplyItem = true;
        else canApplyItem = false;
    }

    public void ReCheckForWarnings(GardenItemType gip)
    {
        ActivateWarning(gip, true);
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
        outline.ChangeOutlineColor(new Color(252f / 256f, 157f / 256f, 3f / 256f), true);
        Instantiate(grownEffect, transform.position, Quaternion.identity);
        source.PlayOneShot(musicManager.GetCurrentMusic().grownPlantClip);
    }

    public IEnumerator TriggerMusicEffect()
    {
        musicPlayer.SetActive(true);
        source.PlayOneShot(musicManager.GetCurrentMusic().phonographClip);
        yield return new WaitForSeconds(musicManager.GetCurrentMusic().phonographClip.length);
        musicPlayer.SetActive(false);
    }

    public void RePlantPlant(Plant p)
    {
        StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("plantplant"));

        p.flowerPotIn.plantInSpace = null;
        p.transform.position = plantPlantingPosition.position;
        p.transform.parent = transform;
        p.SetPlanted();
        p.ApplyColorToPlant(new Color(1f, 1f, 1f, 1f));
        outline.ChangeOutlineColor(Color.white, false);
        UpdatePlantSellPrice(p.GetActualRevenue());
        triggerColl.enabled = false;
        p.plantAnim.speed = 1f;
        p.flowerPotIn = this;
        p.replanting = false;

        if (p != plantInSpace)
            plantInSpace = p;
        
        if (p.ExpectingGardenItem())
            ReCheckForWarnings(p.ExpectedGardenItem());
    }

    public void PlantPlant(Plant p)
    {
        StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("plantplant"));
        GameObject _s = Instantiate(plantsManager.GetSprout(), plantPlantingPosition.position, Quaternion.identity);
        _s.transform.position = plantPlantingPosition.position;
        p.transform.position = plantPlantingPosition.position;
        p.transform.parent = transform;
        p.flowerPotIn = this;
        _s.transform.parent = transform;
        seedDatabase.UsePlant(p.plantData);
        outline.outlineRenderer.enabled = false;
        hoveringPlantIsAccepted = false;
        canUseOutline = false;
        plantInSpace = p;
        UpdatePlantSellPrice(p.GetActualRevenue());
        triggerColl.enabled = false;
        p.ApplyColorToPlant(new Color(1f, 1f, 1f, 1f));
        p.SetPlanted(_s);
        DataCollector.instance.AddPlant(MusicManager.instance.GetCurrentMusic().world, this, p);
        p.plantIsAbove = null;
        p.transform.localScale = Vector3.zero;
    }

    public void PlantPlantFromLoad(GameObject plantPref, PlantSpot plantData)
    {
        GameObject _s = Instantiate(plantsManager.GetSprout(), plantPlantingPosition.position, Quaternion.identity);
        _s.transform.position = plantPlantingPosition.position;
        Plant p = Instantiate(plantPref, plantPref.transform.position, Quaternion.identity).GetComponent<Plant>();
        p.transform.position = plantPlantingPosition.position;
        p.transform.parent = transform;
        p.flowerPotIn = this;
        _s.transform.parent = transform;
        
        if (outline.outlineRenderer == null)
            outline.StartStuff();

        outline.outlineRenderer.enabled = false;
        hoveringPlantIsAccepted = false;
        canUseOutline = false;
        plantInSpace = p;
        UpdatePlantSellPrice(p.GetActualRevenue());
        triggerColl.enabled = false;
        p.ApplyColorToPlant(new Color(1f, 1f, 1f, 1f));
        p.SetPlanted(_s);
        p.plantIsAbove = null;
        p.transform.localScale = Vector3.zero;
        p.SetPlantDataFromLoad(plantData);
    }

    public bool GetIfPlantIsAccepted()
    {
        return hoveringPlantIsAccepted;
    }

    void OnTriggerStay(Collider other) 
    {
        if (other.CompareTag("Plant") && other.GetComponent<Plant>().plantIsAbove == null && canUseOutline)
        {
            other.GetComponent<Plant>().plantIsAbove = this;

            if (other.GetComponent<Plant>().CanPlantInFlowerPot(flowerPotAsset.flowerPotType))
            {
                outline.ChangeOutlineColor(Color.green, true);
                hoveringPlantIsAccepted = true;
            }

            else
            {
                outline.ChangeOutlineColor(Color.red, true);
                hoveringPlantIsAccepted = false;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Plant"))
        {
            other.GetComponent<Plant>().plantIsAbove = null;
            outline.ChangeOutlineColor(Color.white, false);
            hoveringPlantIsAccepted = false;
        }
    }
}
