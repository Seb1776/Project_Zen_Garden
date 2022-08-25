using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Plant : MonoBehaviour
{
    public PlantAsset plantData;

    private GameManager manager;
    private PlantsManager plantsManager;
    private bool planted;
    private bool growth;
    private GameObject _sprout;
    private Collider coll;
    private Transform handPosition;
    private Player player;
    public Animator plantAnim;

    public Vector2Int waterRange, compostRange, fertilizerRange, musicRange;
    private Vector2 timeRange;
    private Vector3 outsideScale;
    private float setTimeRange, currentTimeRange;
    private bool waitingForInteraction;
    private bool progressSet;
    private bool gardenItemChosen, canReplant;
    public bool replanting;
    public bool fullyGrown;
    private int growThreshold;
    private bool selling;
    private int currentGrowThreshold;
    public FlowerPot flowerPotIn, plantIsAbove;
    private GardenItemType? expectedItem = null;
    private GardenItem itemHolding;

    private float revMulIncreaser;
    private float revenueMultiplier;
    private float percentageExtra;

    void Awake()
    {
        plantAnim = GetComponent<Animator>();
        coll = GetComponent<Collider>();

        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        plantsManager = GameObject.FindGameObjectWithTag("PlantsManager").GetComponent<PlantsManager>();

        plantsManager.AssignQualityData(this);

        outsideScale = transform.localScale;
    }

    void Start()
    {
        if (!planted)
        {
            ApplyColorToPlant(new Color(1f, 1f, 1f, .5f));
            plantAnim.speed = 0f;
        }
        
        revenueMultiplier = 1f / GetAccumulativeRange();
        revMulIncreaser = 1f / GetAccumulativeRange();
    }

    void Update()
    {
        LookInCameraDirection();
        PlantGrowBehaviour();

        if (planted)
            PlantProgress();
        
        if (selling)
            SellPlant();
    }

    public void SetPlantDataFromLoad(PlantSpot data)
    {
        planted = false;

        waterRange = new Vector2Int(data.waterCurrentUses, data.waterTotalUses);
        compostRange = new Vector2Int(data.compostCurrentUses, data.compostTotalUses);
        fertilizerRange = new Vector2Int(data.fertilizerCurrentUses, data.fertilizerTotalUses);
        musicRange = new Vector2Int(data.phonographCurrentUses, data.phonographTotalUses);
        currentTimeRange = 0f;

        switch(data.plantLastRequire)
        {
            case "Water":
                expectedItem = GardenItemType.Water;
            break;

            case "Compost":
                expectedItem = GardenItemType.Compost;
            break;

            case "Fertilizer":
                expectedItem = GardenItemType.Fertilizer;
            break;

            case "Music":
                expectedItem = GardenItemType.Music;
            break;
        }

        if (data.plantIsGrown)
            GrowPlant(false);
        
        if (data.plantIsFullyGrown)
        {
            flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
            fullyGrown = true;
            currentTimeRange = 0f;
        }

        planted = true;
    }

    public GardenItemType ExpectedGardenItem()
    {
        return (GardenItemType)expectedItem;
    }

    public void TriggerReplant()
    {   
        if (canReplant)
        {
            DeactivateWarnings();
            transform.parent = null;
            flowerPotIn.triggerColl.enabled = true;
            flowerPotIn.canUseOutline = true;
            flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
            planted = false;
            replanting = true;
            ApplyColorToPlant(new Color(1f, 1f, 1f, .5f));
            plantAnim.speed = 0f;
        }
    }

    public void RemovePlant()
    {
        SoundEffectsManager.instance.PlaySoundEffectNC("removeplant");
        DataCollector.instance.RemovePlant(MusicManager.instance.GetCurrentMusic().world, flowerPotIn);
        DeactivateWarnings();
        flowerPotIn.canUseOutline = true;
        flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
        flowerPotIn.triggerColl.enabled = true;
        Destroy(this.gameObject);
    }

    public void SetHandPosition(Transform _hand)
    {
        handPosition = _hand;
    }

    public int GetActualRevenue()
    {
        if (fullyGrown)
            return (int)(revenueMultiplier * plantData.revenuePrice) + GetExtraRevenue();

        return (int)(revenueMultiplier * plantData.revenuePrice);
    }

    public int GetExtraRevenue()
    {
        return (int)(GameHelper.GetPercentageFromValue(plantData.revenuePrice, percentageExtra));
    }

    public void SetPlantProgress(PlantProcessAsset ppa)
    {
        waterRange.y = Random.Range(ppa.waterRange.x, ppa.waterRange.y);
        compostRange.y = Random.Range(ppa.compostRange.x, ppa.compostRange.y);
        fertilizerRange.y = Random.Range(ppa.fertilizerRange.x, ppa.fertilizerRange.y);
        musicRange.y = Random.Range(ppa.musicRange.x, ppa.musicRange.y);
        timeRange = ppa.timeRange;
        percentageExtra = ppa.plantPercentageExtra;
        growThreshold = (int)((waterRange.y + compostRange.y + fertilizerRange.y + musicRange.y) / 2f);
        GetNewTimeRange();
        progressSet = true;
    }

    void PlantProgress()
    {
        if (progressSet && !fullyGrown)
        {
            if (currentTimeRange < setTimeRange && !waitingForInteraction)
                currentTimeRange += Time.deltaTime;
            
            else if (currentTimeRange >= setTimeRange)
            {
                waitingForInteraction = true;

                if (!gardenItemChosen)
                    ChooseGardenItem();
            }
        }
    }

    void GetNewTimeRange()
    {
        setTimeRange = Random.Range(timeRange.x, timeRange.y);
    }

    public void GrowPlant(bool playSound = true)
    {   
        if (!growth)
        {
            _sprout.GetComponent<Animator>().SetTrigger("hide");
            StartCoroutine(GrowPlantAfterSprout(playSound));
        }
    }

    public bool ExpectingGardenItem()
    {
        return currentTimeRange >= setTimeRange;
    }

    void ChooseGardenItem()
    {
        List<GardenItemType> availableItems = new List<GardenItemType>();

        if (expectedItem == null)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 0 && !ItemIsComplete(waterRange))
                    availableItems.Add(GardenItemType.Water);

                else if (i == 1 && !ItemIsComplete(compostRange))
                    availableItems.Add(GardenItemType.Compost);

                else if (i == 2 && !ItemIsComplete(fertilizerRange))
                    availableItems.Add(GardenItemType.Fertilizer);

                else if (i == 3 && !ItemIsComplete(musicRange))
                    availableItems.Add(GardenItemType.Music);
            }
            
            int randItemIdx = Random.Range(0, availableItems.Count);

            if (availableItems[randItemIdx] == GardenItemType.Water)
            {
                expectedItem = GardenItemType.Water;
                flowerPotIn.ActivateWarning(GardenItemType.Water, true);
            }
            
            if (availableItems[randItemIdx] == GardenItemType.Compost)
            {
                expectedItem = GardenItemType.Compost;
                flowerPotIn.ActivateWarning(GardenItemType.Compost, true);
            }
            
            if (availableItems[randItemIdx] == GardenItemType.Fertilizer)
            {
                expectedItem = GardenItemType.Fertilizer;
                flowerPotIn.ActivateWarning(GardenItemType.Fertilizer, true);
            }
            
            if (availableItems[randItemIdx] == GardenItemType.Music)
            {
                expectedItem = GardenItemType.Music;
                flowerPotIn.ActivateWarning(GardenItemType.Music, true);
            }
        }

        else
        {
            switch (expectedItem)
            {
                case GardenItemType.Water:
                    flowerPotIn.ActivateWarning(GardenItemType.Water, true);
                break;

                case GardenItemType.Compost:
                    flowerPotIn.ActivateWarning(GardenItemType.Compost, true);
                break;

                case GardenItemType.Fertilizer:
                    flowerPotIn.ActivateWarning(GardenItemType.Fertilizer, true);
                break;

                case GardenItemType.Music:
                    flowerPotIn.ActivateWarning(GardenItemType.Music, true);
                break;
            }
        }

        DataCollector.instance.SetPlantGardenState(MusicManager.instance.GetCurrentMusic().world, expectedItem.ToString(), flowerPotIn);
        gardenItemChosen = true;
        expectedItem = null;
    }

    public void DeactivateWarnings()
    {
        flowerPotIn.ActivateWarning(GardenItemType.Water, false);
        flowerPotIn.ActivateWarning(GardenItemType.Compost, false);
        flowerPotIn.ActivateWarning(GardenItemType.Fertilizer, false);
        flowerPotIn.ActivateWarning(GardenItemType.Music, false);
    }

    public void ApplyGardenItem(GardenItemType item)
    {   
        if (item == expectedItem)
        {
            switch (item)
            {
                case GardenItemType.Water:
                    waterRange.x++;

                    DataCollector.instance.SetPlantGardenData(
                        MusicManager.instance.GetCurrentMusic().world, flowerPotIn, expectedItem.ToString(), waterRange.x
                    );
                break;

                case GardenItemType.Compost:
                    compostRange.x++;

                    DataCollector.instance.SetPlantGardenData(
                        MusicManager.instance.GetCurrentMusic().world, flowerPotIn, expectedItem.ToString(), compostRange.x
                    );
                break;

                case GardenItemType.Fertilizer:
                    fertilizerRange.x++;

                    DataCollector.instance.SetPlantGardenData(
                        MusicManager.instance.GetCurrentMusic().world, flowerPotIn, expectedItem.ToString(), fertilizerRange.x
                    );
                break;

                case GardenItemType.Music:
                    musicRange.x++;

                    DataCollector.instance.SetPlantGardenData(
                        MusicManager.instance.GetCurrentMusic().world, flowerPotIn, expectedItem.ToString(), musicRange.x
                    );
                break;
            }

            flowerPotIn.ActivateWarning(item, false);

            if (currentGrowThreshold >= growThreshold && !growth)
                GrowPlant();
            
            else
                currentGrowThreshold++;

            if (!CheckForAllRangesCompleted())
            {
                currentTimeRange = 0f;
                waitingForInteraction = false;
                gardenItemChosen = false;
                expectedItem = null;
                flowerPotIn.outline.ChangeOutlineColor(Color.white, false);

                revenueMultiplier += revMulIncreaser;
                flowerPotIn.UpdatePlantSellPrice(GetActualRevenue());

                GetNewTimeRange();
            }

            else
            {
                flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
                fullyGrown = true;
                currentTimeRange = 0f;
                flowerPotIn.PlayFullGrown();
            }
        }
    }

    public void TriggerSellPlant()
    {
        selling = true;
    }

    public bool CanPlantInFlowerPot(FlowerPotType type)
    {
        return plantData.canBePlantedIn.Contains(type);
    }

    void SellPlant()
    {
        if (selling)
        {   
            if (growth)
            {
                if (Vector2.Distance(transform.localScale, Vector3.zero) > 0.01f)
                {
                    transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 2.5f * Time.deltaTime);
                    flowerPotIn.revenueText.gameObject.SetActive(false);
                    flowerPotIn.sellPlantButton.SetActive(false);
                }

                else
                {
                    flowerPotIn.canUseOutline = true;
                    flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
                    flowerPotIn.triggerColl.enabled = true;
                    flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
                    Destroy(this.gameObject);
                }
            }

            else
            {
                _sprout.GetComponent<Animator>().SetTrigger("hide");
                flowerPotIn.revenueText.gameObject.SetActive(false);
                flowerPotIn.sellPlantButton.SetActive(false);
                DeactivateWarnings();
                StartCoroutine(SellSprout());
            }
        }
    }

    bool CheckForAllRangesCompleted()
    {
        if (ItemIsComplete(waterRange) && ItemIsComplete(compostRange) && ItemIsComplete(fertilizerRange)
            && ItemIsComplete(musicRange))
            {
                return true;
            }
        
        return false;
    }
    
    int GetAccumulativeRange()
    {
        return waterRange.y + compostRange.y + fertilizerRange.y + musicRange.y;
    }

    bool ItemIsComplete(Vector2Int range)
    {
        if (range.x >= range.y)
            return true;
        
        return false;
    }

    void PlantGrowBehaviour()
    {
        if (!planted && handPosition != null)
            transform.position = handPosition.position;
        
        if (!fullyGrown && growth && !replanting && transform.localScale != plantData.initialScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, plantData.initialScale, 2 * Time.deltaTime);

            if (Vector3.Distance(transform.localScale, plantData.initialScale) <= 0.001f)
            {
                transform.localScale = plantData.initialScale;
                canReplant = true;
            }
        }
    }

    IEnumerator GrowPlantAfterSprout(bool playSound = true)
    {   
        if (!growth)
        {
            yield return new WaitForSeconds(1.167f);
            growth = true;
            if (playSound) StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("plantgrow"));
        }
    }

    IEnumerator SellSprout()
    {
        yield return new WaitForSeconds(1.167f);
        flowerPotIn.canUseOutline = true;
        flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
        flowerPotIn.triggerColl.enabled = true;
        Destroy(_sprout.gameObject);
        Destroy(this.gameObject);
    }

    public void ApplyColorToPlant(Color c)
    {
        foreach (SpriteRenderer sr in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.color = c;
        }

        plantAnim.speed = 1f;
    }

    public void SetPlanted(GameObject _sp = null)
    {
        if (_sp != null)
            _sprout = _sp;

        planted = true;
    }

    void LookInCameraDirection()
    {
        transform.LookAt(manager.GetMainCamera().transform);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f));
    }

    void OnTriggerStay(Collider c)
    {
        if (c.transform.CompareTag("FlowerPot"))
        {
            if (c.GetComponent<FlowerPot>() != null && c.isTrigger)
                player.HoveringAFlowerPot(c.GetComponent<FlowerPot>());
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.transform.CompareTag("FlowerPot"))
            if (c.GetComponent<FlowerPot>() != null && c.isTrigger)
                player.HoveringAFlowerPot(null);
    }
}
