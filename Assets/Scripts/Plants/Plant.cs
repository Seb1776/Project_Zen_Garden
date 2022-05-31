using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Plant : MonoBehaviour
{
    public PlantAsset plantData;
    [SerializeField] private AnimationClip sproutDeAppearAnimation;

    private GameManager manager;
    private PlantsManager plantsManager;
    private bool planted;
    private bool growth;
    private GameObject _sprout;
    private Collider coll;
    private Transform handPosition;
    private Player player;
    private Animator plantAnim;

    private Vector2Int waterRange, compostRange, fertilizerRange, musicRange;
    private Vector2 timeRange;
    private float setTimeRange, currentTimeRange;
    private bool waitingForInteraction;
    private bool progressSet;
    private bool gardenItemChosen;
    private bool fullyGrown;
    private int growThreshold;
    private int currentGrowThreshold;
    private GardenItemType? expectedItem = null;
    private GardenItem itemHolding;

    void Awake()
    {
        plantAnim = GetComponent<Animator>();
        coll = GetComponent<Collider>();

        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        plantsManager = GameObject.FindGameObjectWithTag("PlantsManager").GetComponent<PlantsManager>();

        plantAnim.speed = 0f;

        plantsManager.AssignQualityData(this);
    }

    void Start()
    {   
        if (!planted)
            TransparentPlant();
    }

    void Update()
    {
        LookInCameraDirection();
        PlantGrowBehaviour();
        PlantProgress();
        NonVRDebug();
    }

    void NonVRDebug()
    {
        if (waitingForInteraction)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
                ApplyGardenItem(GardenItemType.Water);
            
            if (Keyboard.current.wKey.wasPressedThisFrame)
                ApplyGardenItem(GardenItemType.Compost);
            
            if (Keyboard.current.eKey.wasPressedThisFrame)
                ApplyGardenItem(GardenItemType.Fertilizer);

            if (Keyboard.current.rKey.wasPressedThisFrame)
                ApplyGardenItem(GardenItemType.Music);

        }
    }

    public void SetHandPosition(Transform _hand)
    {
        handPosition = _hand;
    }

    public void SetPlantProgress(PlantProcessAsset ppa)
    {
        waterRange.y = Random.Range(ppa.waterRange.x, ppa.waterRange.y);
        compostRange.y = Random.Range(ppa.compostRange.x, ppa.compostRange.y);
        fertilizerRange.y = Random.Range(ppa.fertilizerRange.x, ppa.fertilizerRange.y);
        musicRange.y = Random.Range(ppa.musicRange.x, ppa.musicRange.y);
        timeRange = ppa.timeRange;
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
        setTimeRange = 2f;//Random.Range(timeRange.x, timeRange.y);
    }

    void GrowPlant()
    {   
        if (!growth)
        {
            _sprout.GetComponent<Animator>().SetTrigger("hide");
            StartCoroutine(GrowPlantAfterSprout());
        }
    }

    void ChooseGardenItem()
    {
        List<GardenItemType> availableItems = new List<GardenItemType>();

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
            expectedItem = GardenItemType.Water;
        
        if (availableItems[randItemIdx] == GardenItemType.Compost)
            expectedItem = GardenItemType.Compost;
        
        if (availableItems[randItemIdx] == GardenItemType.Fertilizer)
            expectedItem = GardenItemType.Fertilizer;
        
        if (availableItems[randItemIdx] == GardenItemType.Music)
            expectedItem = GardenItemType.Music;

        Debug.Log(expectedItem + " chosen");
        gardenItemChosen = true;
    }

    public void ApplyGardenItem(GardenItemType item)
    {   
        if (item == expectedItem)
        {
            switch (item)
            {
                case GardenItemType.Water:
                    waterRange.x++;
                break;

                case GardenItemType.Compost:
                    compostRange.x++;
                break;

                case GardenItemType.Fertilizer:
                    fertilizerRange.x++;
                break;

                case GardenItemType.Music:
                    musicRange.x++;
                break;
            }

            currentGrowThreshold++;

            if (currentGrowThreshold >= growThreshold)
                GrowPlant();

            if (!CheckForAllRangesCompleted())
            {
                currentTimeRange = 0f;
                waitingForInteraction = false;
                gardenItemChosen = false;
                expectedItem = null;
                GetNewTimeRange();
            }

            else
                fullyGrown = true;
        }
    }

    bool CheckForAllRangesCompleted()
    {
        if (ItemIsComplete(waterRange) && ItemIsComplete(compostRange) && ItemIsComplete(fertilizerRange)
            && ItemIsComplete(musicRange))
                return true;
        
        return false;
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
        
        if (Keyboard.current.kKey.wasPressedThisFrame)
            GrowPlant();
        
        if (growth)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, plantData.initialScale, 2 * Time.deltaTime);

            if (Vector3.Distance(transform.localScale, plantData.initialScale) <= 0.001f)
            {
                transform.localScale = plantData.initialScale;
                growth = false;
            }
        }
    }

    IEnumerator GrowPlantAfterSprout()
    {
        yield return new WaitForSeconds(sproutDeAppearAnimation.length);
        Destroy(_sprout.gameObject);
        growth = true;
    }

    void TransparentPlant()
    {
        foreach (SpriteRenderer sr in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            Color transp = new Color(1f, 1f, 1f, .5f);
            sr.color = transp;
        }
    }

    public void ApplyColorToPlant()
    {
       foreach (SpriteRenderer sr in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            Color transp = new Color(1f, 1f, 1f, 1f);
            sr.color = transp;
        }

        transform.localScale = Vector3.zero;
        plantAnim.speed = 1f;
    }

    public void SetPlanted(GameObject _sp)
    {
        _sprout = _sp;
        planted = true;
    }

    void LookInCameraDirection()
    {
        transform.LookAt(manager.GetMainCamera().transform);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f));
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.transform.CompareTag("FlowerPot"))
            if (c.GetComponent<FlowerPot>() != null && c.isTrigger)
                player.HoveringAFlowerPot(c.GetComponent<FlowerPot>());
    }

    void OnTriggerExit(Collider c)
    {
        if (c.transform.CompareTag("FlowerPot"))
            if (c.GetComponent<FlowerPot>() != null && c.isTrigger)
                player.HoveringAFlowerPot(null);
    }
}
