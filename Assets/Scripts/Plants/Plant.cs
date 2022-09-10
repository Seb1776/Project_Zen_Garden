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
    public bool growth;
    private GameObject _sprout;
    private Collider coll;
    private Transform handPosition;
    private Player player;
    public Animator plantAnim;

    //New Progress
    public float currentProgressTime;
    [SerializeField] private int maxTimeToProduce;
    [SerializeField] private int coinsToProduce;
    public float currentEnergyTime;
    public float currentLifeTime;
    [SerializeField] private bool hasEnergy = true;
    public int currentPlantLevelIndex;
    public bool isDeco;

    //Old Progress
    private Vector3 outsideScale;
    private bool gardenItemChosen, canReplant;
    public bool replanting;
    private bool selling;
    public FlowerPot flowerPotIn, plantIsAbove;
    public GardenItemType expectedItem = GardenItemType.None;
    private GardenItem itemHolding;

    void Awake()
    {
        plantAnim = GetComponent<Animator>();
        coll = GetComponent<Collider>();

        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        plantsManager = GameObject.FindGameObjectWithTag("PlantsManager").GetComponent<PlantsManager>();

        outsideScale = transform.localScale;
    }

    void Start()
    {
        if (!planted)
        {
            ApplyColorToPlant(new Color(1f, 1f, 1f, .5f));
            plantAnim.speed = 0f;
        }
    }

    void Update()
    {
        LookInCameraDirection();
        PlantGrowBehaviour();

        if (selling)
            SellPlant();
    }

    public void SetPlantDataFromLoad(PlantSpot data)
    {
        planted = false;

        if (data.plantIsGrown)
            GrowPlant(false);

        switch (data.plantLastRequire)
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

            default:
                expectedItem = GardenItemType.None;
                break;
        }

        planted = true;

        if (data.plantLevelIndex != 3)
        {
            currentPlantLevelIndex = data.plantLevelIndex;
            currentEnergyTime = data.plantLastEnergyTick;
            currentProgressTime = data.plantLastProducingTick;
            currentLifeTime = data.plasntLastLifeTick;

            maxTimeToProduce = plantData.plantLevels[currentPlantLevelIndex].producingTime;

            flowerPotIn.SetFlowerPotUI(plantData.plantName, currentPlantLevelIndex + 1, plantData.plantLevels[currentPlantLevelIndex].
                energyLevel, plantData.plantLevels[currentPlantLevelIndex].producingTime, plantData.plantLevels[currentPlantLevelIndex].plantLife);
            flowerPotIn.UpdateFlowerPotUI((true, currentPlantLevelIndex + 1), (true, currentEnergyTime), (true, currentProgressTime), (true, (int)currentLifeTime));
            flowerPotIn.ToggleFlowerPotUI(true);

            if (expectedItem != GardenItemType.None)
            {
                flowerPotIn.ToggleFlowerPotSliders(false);
                hasEnergy = false;
                ChooseGardenItem();
            }
        }

        else SetPlantAsDeco();
    }

    public void GrowPlant(bool playSound = true, bool triggerFullGrown = false)
    {
        if (!growth)
        {
            _sprout.GetComponent<Animator>().SetTrigger("hide");
            StartCoroutine(GrowPlantAfterSprout(playSound, triggerFullGrown));
        }
    }

    IEnumerator GrowPlantAfterSprout(bool playSound = true, bool triggerFullyGrown = false)
    {
        if (!growth)
        {
            yield return new WaitForSeconds(1.167f);

            growth = true;

            if (playSound) StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect("plantgrow"));

            if (triggerFullyGrown)
            {
                yield return new WaitForSeconds(1.167f + 1f);
                flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
                SetPlanted();
                ApplyColorToPlant(new Color(1f, 1f, 1f, 1f));
                plantAnim.speed = 1f;
            }

            else
            {
                DataCollector.instance.SetPlantFullGrownData(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), flowerPotIn, growth);
            }
        }
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
            flowerPotIn.ToggleFlowerPotUI(false);
            flowerPotIn.triggerColl.enabled = true;
            flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
            planted = false;
            replanting = true;
            PlantsManager.OnTick -= PlantProgress;
            ApplyColorToPlant(new Color(1f, 1f, 1f, .5f));
            plantAnim.speed = 0f;
        }
    }

    public void RemovePlant()
    {
        SoundEffectsManager.instance.PlaySoundEffectNC("removeplant");
        DataCollector.instance.RemovePlant(MusicManager.instance.GetCurrentMusic().world, flowerPotIn);
        DeactivateWarnings();
        flowerPotIn.ToggleFlowerPotUI(false);
        flowerPotIn.canUseOutline = true;
        flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
        flowerPotIn.triggerColl.enabled = true;
        Destroy(this.gameObject);
    }

    public void SetHandPosition(Transform _hand)
    {
        handPosition = _hand;
    }

    public void SetPlantProgress()
    {
        PlantLevel pl = plantData.GetPlantLevel(0);

        maxTimeToProduce = pl.producingTime;
        currentEnergyTime = pl.energyLevel;
        coinsToProduce = pl.producedCoins;
        currentLifeTime = pl.plantLife;
        currentPlantLevelIndex = 0;
    }

    public void PlantProgress(object sender, PlantsManager.OnTickEventArgs e)
    {
        if (planted && hasEnergy)
        {
            currentProgressTime += 1;
            flowerPotIn.UpdateFlowerPotUI((false, currentPlantLevelIndex + 1), (false, currentEnergyTime), (true, currentProgressTime), (false, 0));
            DataCollector.instance.SetPlantProduceTick(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), flowerPotIn, currentProgressTime);

            if (currentProgressTime >= maxTimeToProduce)
            {
                currentProgressTime = 0f;

                currentEnergyTime -= 1;
                flowerPotIn.TriggerCoinRevenue(plantData.GetPlantLevel(currentPlantLevelIndex).producedCoins);

                if (GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn) == MusicManager.instance.GetCurrentMusic().world)
                {
                    PlantsManager.instance.AddMoneyToWorldBank(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), plantData.GetPlantLevel(currentPlantLevelIndex).producedCoins);
                    DataCollector.instance.SetWorldBankMoney(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), PlantsManager.instance.GetCurrentWorldMoney(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn)));
                }

                else PlantsManager.instance.AddMoneyWorldChange(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), plantData.GetPlantLevel(currentPlantLevelIndex).producedCoins);

                flowerPotIn.UpdateFlowerPotUI((false, currentPlantLevelIndex + 1), (true, currentEnergyTime), (true, currentProgressTime), (false, 0));
                DataCollector.instance.SetPlantEnergyTick(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), flowerPotIn, currentEnergyTime);

                if (currentEnergyTime <= 0)
                {
                    hasEnergy = false;
                    currentLifeTime -= 1;
                    DataCollector.instance.SetPlantLifeTick(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), flowerPotIn, currentLifeTime);

                    if (currentLifeTime > 0)
                    {
                        ChooseGardenItem();

                        PlantsManager.instance.AddPlantWorldChange(
                            GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), plantData.name, true, true
                        );
                    }
                    
                    else
                    {
                        PlantsManager.instance.AddPlantWorldChange(
                            GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), plantData.name, true, false
                        );

                        PlantsManager.instance.AddPlantWorldChange(
                            GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), plantData.name, false, true
                        );

                        WitherPlant();
                    }
                }
            }
        }
    }

    public bool ExpectingGardenItem()
    {
        return gardenItemChosen;
    }

    void ChooseGardenItem()
    {
        List<GardenItemType> availableItems = new List<GardenItemType>();

        if (expectedItem == GardenItemType.None)
        {
            int randomItem = Random.Range(0, plantData.plantQuality.canAskFor.Length);
            expectedItem = plantData.plantQuality.canAskFor[randomItem];
        }

        flowerPotIn.ToggleFlowerPotSliders(false);
        flowerPotIn.ActivateWarning(expectedItem, true);

        DataCollector.instance.SetPlantGardenState(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), expectedItem.ToString(), flowerPotIn);
        gardenItemChosen = true;
    }

    public void UpgradePlant()
    {   
        if (CanUpgradePlant())
        {   
            if (Player.instance.CanSpendMoney(plantData.plantLevels[currentPlantLevelIndex + 1].upgradePrice))
            {
                currentPlantLevelIndex++;
                flowerPotIn.PlayGrowSFX();

                if (currentPlantLevelIndex >= (plantData.plantLevels.Length - 1) / 2f)
                    GrowPlant();

                if (currentPlantLevelIndex == plantData.plantLevels.Length - 1)
                    DataCollector.instance.SetPlantFullGrownData(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), flowerPotIn, growth);

                ResetPlantTimers();
                flowerPotIn.SetFlowerPotUI(plantData.plantName, currentPlantLevelIndex + 1, plantData.GetPlantLevel(currentPlantLevelIndex).energyLevel, plantData.GetPlantLevel(currentPlantLevelIndex).producingTime, plantData.GetPlantLevel(currentPlantLevelIndex).plantLife);
                flowerPotIn.UpdateFlowerPotUI((true, currentPlantLevelIndex + 1), (true, currentEnergyTime), (true, currentProgressTime), (true, (int)currentLifeTime));
                DataCollector.instance.SetPlantLevel(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), flowerPotIn, currentPlantLevelIndex);
                Player.instance.SpendMoney(plantData.plantLevels[currentPlantLevelIndex].upgradePrice);
            }
        }
    }

    public void WitherPlant()
    {
        TriggerSellPlant();
    }

    public void ResetPlantTimers()
    {
        currentProgressTime = 0f;

        PlantLevel pl = plantData.GetPlantLevel(currentPlantLevelIndex);

        maxTimeToProduce = pl.producingTime;
        currentEnergyTime = pl.energyLevel;
        coinsToProduce = pl.producedCoins;
    }

    public bool CanUpgradePlant()
    {
        return currentPlantLevelIndex < plantData.plantLevels.Length - 1;
    }

    public void DeactivateWarnings()
    {
        flowerPotIn.ActivateWarning(GardenItemType.Water, false);
        flowerPotIn.ActivateWarning(GardenItemType.Compost, false);
        flowerPotIn.ActivateWarning(GardenItemType.Fertilizer, false);
        flowerPotIn.ActivateWarning(GardenItemType.Music, false);
    }

    public void ReFillEnergy()
    {
        currentEnergyTime = plantData.GetPlantLevel(currentPlantLevelIndex).energyLevel;
        hasEnergy = true;
    }

    public void ApplyGardenItem(GardenItemType item)
    {
        if (item == expectedItem)
        {
            flowerPotIn.ActivateWarning(expectedItem, false);
            expectedItem = GardenItemType.None;
            DataCollector.instance.SetPlantGardenState(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), expectedItem.ToString(), flowerPotIn);
            PlantsManager.instance.AddPlantWorldChange(
                GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), plantData.name, true, false
            );
            gardenItemChosen = false;
            ReFillEnergy();
            flowerPotIn.ToggleFlowerPotSliders(true);
            flowerPotIn.UpdateFlowerPotUI((false, currentPlantLevelIndex + 1), (true, currentEnergyTime), (true, currentProgressTime), (true, (int)currentLifeTime));
        }
    }

    public void SetPlantAsDeco()
    {
        flowerPotIn.PlayGrowSFX();
        PlantsManager.OnTick -= PlantProgress;
        isDeco = true;
        DataCollector.instance.SetPlantLevel(GameManager.instance.GetGameWorldFromString(flowerPotIn.createdIn), flowerPotIn, 3);
        expectedItem = GardenItemType.None;
        flowerPotIn.outline.ChangeOutlineColor(new Color32(250, 114, 2, 255), true);
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
                    flowerPotIn.actionButtons.SetActive(false);
                }

                else
                {
                    DataCollector.instance.RemovePlant(MusicManager.instance.GetCurrentMusic().world, flowerPotIn);
                    flowerPotIn.canUseOutline = true;
                    flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
                    flowerPotIn.triggerColl.enabled = true;
                    flowerPotIn.outline.ChangeOutlineColor(Color.white, false);
                    flowerPotIn.ToggleFlowerPotUI(false);
                    Destroy(this.gameObject);
                }
            }

            else
            {
                _sprout.GetComponent<Animator>().SetTrigger("hide");
                flowerPotIn.actionButtons.SetActive(false);
                DataCollector.instance.RemovePlant(MusicManager.instance.GetCurrentMusic().world, flowerPotIn);
                flowerPotIn.ToggleFlowerPotUI(false);
                DeactivateWarnings();
                StartCoroutine(SellSprout());
            }
        }
    }

    void PlantGrowBehaviour()
    {
        if (!planted && handPosition != null)
            transform.position = handPosition.position;

        if (growth && !replanting && transform.localScale != plantData.initialScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, plantData.initialScale, 2 * Time.deltaTime);

            if (Vector3.Distance(transform.localScale, plantData.initialScale) <= 0.001f)
            {
                transform.localScale = plantData.initialScale;
                canReplant = true;
            }
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

        flowerPotIn.ToggleFlowerPotUI(true);
        flowerPotIn.SetFlowerPotUI(plantData.plantName, currentPlantLevelIndex + 1, plantData.GetPlantLevel(currentPlantLevelIndex).energyLevel, plantData.GetPlantLevel(currentPlantLevelIndex).producingTime, plantData.GetPlantLevel(currentPlantLevelIndex).plantLife);
        flowerPotIn.UpdateFlowerPotUI((true, currentPlantLevelIndex + 1), (true, currentEnergyTime), (true, currentProgressTime), (true, (int)currentLifeTime));
        PlantsManager.OnTick += PlantProgress;
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
        {
            if (c.GetComponent<FlowerPot>() != null && c.isTrigger)
                player.HoveringAFlowerPot(null);
        }
    }
}
