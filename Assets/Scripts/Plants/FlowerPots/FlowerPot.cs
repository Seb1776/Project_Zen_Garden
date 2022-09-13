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
    public GameObject actionButtons, sellPlantPanel, upgradePlantPanel, decoPlantPanel, normalUPlantPanel;
    public Text upgradeProdTime, upgradeCoins, upgradeEnergy, upgradeLifeTime;
    private Animator coinShowAnimator;

    [Header ("UI")]
    [SerializeField] private GameObject needWaterIcon;
    [SerializeField] private GameObject needCompostIcon, needFertilizerIcon, needMusicIcon;
    [SerializeField] private Slider revenueSlider, plantEnergySlider, lifeSlider;
    public Text plantName, plantLevel, plantSellT, plantUpgradeT;

    [Header ("Effect")]
    [SerializeField] private ParticleSystem grownEffect;
    
    private Player player;
    private Collider coll;
    public Collider triggerColl;
    private PlantsManager plantsManager;
    private Plant plantInSpace;
    private SeedDatabase seedDatabase;
    private AudioSource source;
    private MusicManager musicManager;
    public XRGrabInteractable potInteractable;
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
        coinShowAnimator = transform.GetChild(6).transform.GetChild(8).GetComponent<Animator>();
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
            Vector3.Distance(transform.localPosition, startPos) > 0.01f)
                returnToPos = true;
        
        if (returnToPos && MusicManager.instance.currentWorld.ToString() == createdIn)
            GetBackPos();
        
        potInteractable.enabled = !returning;
    }

    void GetBackPos()
    {
        transform.position = Vector3.Lerp(transform.position, startPos, 2.5f * Time.deltaTime);
        returning = true;

        if (Vector3.Distance(transform.position, startPos) <= 0.001f)
        {
            transform.position = startPos;
            returnToPos = false;
            returning = false;
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
            actionButtons.SetActive(true);
        }
    }

    public void TriggerCoinRevenue(int revenue)
    {
        coinShowAnimator.transform.GetChild(1).GetComponent<Text>().text = "+ " + revenue.ToString("N0");
        coinShowAnimator.SetTrigger("showrev");
    }

    public void UnSendFlowerPotToPlayer(XRBaseInteractor flower)
    {
        if (reAssignable)
        {
            DataCollector.instance.UdpateFlowerPotHolder(GameManager.instance.GetGameWorldFromString(createdIn), inPositionOfHolder, hoveringHolder);
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

        Player.instance.DeTriggerPanels();
        triggerColl.enabled = plantInSpace == null;
        SeedDatabase.instance.TriggerHolders(false);
        player.SetHoldFlowerPot();
        actionButtons.SetActive(false);
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
        ToggleFlowerPotSliders(false);
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

    public void PlayGrowSFX()
    {
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
        p.flowerPotIn.triggerColl.enabled = true;
        p.flowerPotIn.plantInSpace = null;
        p.transform.position = plantPlantingPosition.position;
        p.transform.parent = transform;
        p.ApplyColorToPlant(new Color(1f, 1f, 1f, 1f));
        outline.ChangeOutlineColor(Color.white, false);
        triggerColl.enabled = false;
        p.plantAnim.speed = 1f;
        p.flowerPotIn = this;
        p.SetPlanted();
        p.flowerPotIn.SetFlowerPotUI(p.plantData.plantName, p.currentPlantLevelIndex + 1, p.plantData.plantLevels[p.currentPlantLevelIndex].
            energyLevel, p.plantData.plantLevels[p.currentPlantLevelIndex].producingTime, p.plantData.plantLevels[p.currentPlantLevelIndex].plantLife);
        p.flowerPotIn.UpdateFlowerPotUI((true, p.currentPlantLevelIndex + 1), (true, p.currentEnergyTime), (true, p.currentProgressTime), (true, (int)p.currentLifeTime));
        p.flowerPotIn.ToggleFlowerPotUI(true);
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
        outline.StartStuff();
        outline.outlineRenderer.enabled = false;
        hoveringPlantIsAccepted = false;
        canUseOutline = false;
        plantInSpace = p;
        triggerColl.enabled = false;
        p.ApplyColorToPlant(new Color(1f, 1f, 1f, 1f));
        p.SetPlanted(_s);
        p.SetPlantProgress();
        p.expectedItem = GardenItemType.None;
        DataCollector.instance.AddPlant(MusicManager.instance.GetCurrentMusic().world, this, p);
        DataCollector.instance.UpdateSeedPacket(p.plantData.name, SeedDatabase.instance.GetPlantInList(p.plantData).amount);
        p.plantIsAbove = null;
        p.transform.localScale = Vector3.zero;
    }

    public void ToggleFlowerPotUI(bool toggle)
    {   
        if (!plantInSpace.ExpectingGardenItem())
        {
            plantEnergySlider.gameObject.SetActive(toggle);
            revenueSlider.gameObject.SetActive(toggle);
            lifeSlider.gameObject.SetActive(toggle);
        }

        else
            ActivateWarning(plantInSpace.expectedItem, true);

        plantName.gameObject.SetActive(toggle);
        plantLevel.gameObject.SetActive(toggle);
    }

    public void ToggleFlowerPotSliders(bool toggle)
    {
        plantEnergySlider.gameObject.SetActive(toggle);
        revenueSlider.gameObject.SetActive(toggle);
        lifeSlider.gameObject.SetActive(toggle);
    }

    public void SetFlowerPotUI(string _plantName, int _plantLevel, float maxEnergySlide, float maxRevenueSlide, float maxLifeSlide)
    {
        plantName.text = _plantName;
        plantLevel.text = "Level " + _plantLevel;
        plantEnergySlider.maxValue = maxEnergySlide;
        revenueSlider.maxValue = maxRevenueSlide;
        lifeSlider.maxValue = maxLifeSlide;
    }

    public void UpdateFlowerPotUI((bool changeLevel, int newPlantLevel) _plantLevel, (bool changeEnergy, float newEnergy) currentEnergy, 
        (bool changeRevenue, float newRevenue) currentRevenue, (bool changeLife, int newLife) currentLife)
    {
        if (_plantLevel.changeLevel)
            plantLevel.text = "Level " + _plantLevel.newPlantLevel;

        if (currentEnergy.changeEnergy)
            plantEnergySlider.value = currentEnergy.newEnergy;

        if (currentRevenue.changeRevenue)
            revenueSlider.value = currentRevenue.newRevenue;
        
        if (currentLife.changeLife)
            lifeSlider.value = currentLife.newLife;
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
        if (other.CompareTag("Plant") && other.GetComponent<Plant>().plantIsAbove == null && canUseOutline && createdIn == MusicManager.instance.GetCurrentMusic().world.ToString())
        {
            other.GetComponent<Plant>().plantIsAbove = this;
            Debug.Log(other.gameObject.name);

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
