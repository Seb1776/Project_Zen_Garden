using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravelManager : MonoBehaviour
{
    public static TimeTravelManager instance;

    [NonReorderable]
    [SerializeField] private AgeTime[] worlds;
    [SerializeField] private Animator transition;
    [SerializeField] private GameObject currentProps;
    [SerializeField] private GameObject currentTables;
    [SerializeField] private float worldTablesYOffset;
    [SerializeField] private GameObject normalTeleport, bigTeleport;
    [SerializeField] Vector3 canvasNormalPos, canvasModernDayPos, gardenCanvasNormalPos, gardenCanvasMDPos;
    [SerializeField] private float modernDayGardenTablesYOffset;
    [SerializeField] private GardenItem[] mdGardenItems;
    [SerializeField] private GardenItem[] normalGardenItems;
    [SerializeField] private GardenItem[] tutorialGardenItems;
    [Header ("Present Decos")]
    [SerializeField] private GameObject frontYard; 
    [SerializeField] private GameObject backYard, roofYard;

    private bool loading, doneLoading;


    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void ChangeScenario(string _world, bool present)
    {
        loading = true;
        AgeTime selectedAge = null;

        foreach (AgeTime at in worlds)
        {
            if (_world == at.ageID)
            {
                selectedAge = at;
                break;
            }
        }

        if (!present)
        {
            if (selectedAge != null)
            {
                currentProps.SetActive(false);
                RenderSettings.skybox = selectedAge.skyboxMaterial;

                Player.instance.transform.localPosition = new Vector3(1.26699996f, 0f, 1.24300003f);

                if (selectedAge.worldMusic.world == GameWorlds.ModernDay)
                {
                    GameManager.instance.gameCanvas.transform.localPosition = canvasModernDayPos;
                    GameManager.instance.gardenCanvas.transform.localPosition = gardenCanvasMDPos;

                    foreach (GardenItem gi in mdGardenItems)
                        gi.ignorePos = true;
                    
                    foreach (GardenItem gi in normalGardenItems)
                        gi.ignorePos = false;

                    bigTeleport.SetActive(true);
                    normalTeleport.SetActive(false);
                }

                else
                {
                    GameManager.instance.gameCanvas.transform.localPosition = canvasNormalPos;
                    GameManager.instance.gardenCanvas.transform.localPosition = gardenCanvasNormalPos;

                    if (selectedAge.worldMusic.world != GameWorlds.Tutorial)
                    {
                        foreach (GardenItem gi in mdGardenItems)
                            gi.ignorePos = false;
                        
                        foreach (GardenItem gi in normalGardenItems)
                            gi.ignorePos = true;
                    }

                    else
                        foreach (GardenItem gi in tutorialGardenItems)
                            gi.ignorePos = false;

                    bigTeleport.SetActive(false);
                    normalTeleport.SetActive(true);
                }

                if (currentTables.gameObject.name != "MDGarden")
                    currentTables.transform.localPosition += new Vector3(0f, worldTablesYOffset, 0f);
                else
                    currentTables.transform.localPosition += new Vector3(0f, modernDayGardenTablesYOffset, 0f);

                selectedAge.props.SetActive(true);
                currentProps = selectedAge.props;
                currentTables = selectedAge.worldTables;

                if (selectedAge.worldMusic.world != GameWorlds.ModernDay)
                    currentTables.transform.localPosition -= new Vector3(0f, worldTablesYOffset, 0f);
                else
                    currentTables.transform.localPosition -= new Vector3(0f, modernDayGardenTablesYOffset, 0f);

                RenderSettings.ambientSkyColor = selectedAge.sky;
                RenderSettings.ambientEquatorColor = selectedAge.equator;
                RenderSettings.ambientGroundColor = selectedAge.ground;
            }
        }

        else
        {   
            if (_world == "front" || _world == "back" || _world == "roof")
            {
                currentProps.SetActive(false);

                switch(_world)
                {
                    case "front":
                        frontYard.SetActive(true);
                        currentProps = frontYard;
                    break;

                    case "back":
                        backYard.SetActive(true);
                        currentProps = backYard;
                    break;

                    case "roof":
                        roofYard.SetActive(true);
                        currentProps = roofYard;
                    break;
                }
            }
        }

        loading = false;
    }

    public void SetAsEntered(string id)
    {
        for (int i = 0; i < worlds.Length; i++)
            if (worlds[i].ageID == id)
                if (!worlds[i].hasEntered)
                    worlds[i].hasEntered = true;
    }

    public bool HasEnteredBefore(string id)
    {
        for (int i = 0; i < worlds.Length; i++)
            if (worlds[i].ageID == id)
                return worlds[i].hasEntered;
        
        return false;
    }

    public void PlayerEnteredWorld(string id)
    {
        for (int i = 0; i < worlds.Length; i++)
            if (worlds[i].ageID == id)
                worlds[i].hasEntered = true;
    }

    public void TriggerTransition(bool trigger)
    {
        transition.SetBool("warp", trigger);
    }
}

[System.Serializable]
public class AgeTime
{
    public string ageID;
    [ColorUsage(false, true)]
    public Color sky, equator, ground;
    public bool hasEntered;
    public MusicAsset worldMusic;
    public GameObject props;
    public Material skyboxMaterial;
    public GameObject worldTables;
}
