using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravelManager : MonoBehaviour
{
    public static TimeTravelManager instance;

    [NonReorderable]
    public AgeTime[] worlds;
    [SerializeField] private Animator transition;
    [SerializeField] private GameObject currentProps;
    [SerializeField] private GameObject currentTables;
    public float worldTablesYOffset;
    [SerializeField] private GameObject normalTeleport, bigTeleport;
    [SerializeField] Vector3 canvasNormalPos, canvasModernDayPos, gardenCanvasNormalPos, gardenCanvasMDPos;
    public float modernDayGardenTablesYOffset;
    [SerializeField] private GardenItem[] mdGardenItems;
    [SerializeField] private GardenItem[] normalGardenItems;
    [SerializeField] private GardenItem[] tutorialGardenItems;
    [Header("Present Decos")]
    [SerializeField] private GameObject frontYard;
    [SerializeField] private GameObject backYard, roofYard;

    private bool loading, doneLoading;


    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void ChangeScenario(string _world, bool selection)
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
                {
                    gi.ignorePos = false;
                }

                foreach (GardenItem gi in normalGardenItems)
                {
                    gi.ignorePos = true;
                    gi.UpdateReturnPosition(new Vector3(gi.transform.position.x, gi.transform.position.y + worldTablesYOffset, gi.transform.position.z));
                }

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
                    {
                        gi.ignorePos = true;
                        gi.UpdateReturnPosition(new Vector3(gi.transform.localPosition.x, gi.transform.localPosition.y + modernDayGardenTablesYOffset, gi.transform.localPosition.z));
                    }

                    foreach (GardenItem gi in normalGardenItems)
                    {
                        gi.ignorePos = false;
                    }
                }

                else
                {
                    foreach (GardenItem gi in tutorialGardenItems)
                        gi.ignorePos = false;
                    
                    foreach (GardenItem gi in mdGardenItems)
                    {
                        gi.ignorePos = true;
                        gi.UpdateReturnPosition(new Vector3(gi.transform.localPosition.x, gi.transform.localPosition.y + modernDayGardenTablesYOffset, gi.transform.localPosition.z));
                    }

                    foreach (GardenItem gi in normalGardenItems)
                    {
                        gi.ignorePos = true;
                        gi.UpdateReturnPosition(new Vector3(gi.transform.position.x, gi.transform.position.y + worldTablesYOffset, gi.transform.position.z));
                    }
                }

                bigTeleport.SetActive(false);
                normalTeleport.SetActive(true);
            }

            if (selection)
            {
                if (currentTables.gameObject.name != "MDGarden")
                    currentTables.transform.localPosition += new Vector3(0f, worldTablesYOffset, 0f);
                else
                    currentTables.transform.localPosition += new Vector3(0f, modernDayGardenTablesYOffset, 0f);
            }

            selectedAge.props.SetActive(true);
            currentProps = selectedAge.props;
            currentTables = selectedAge.worldTables;

            if (selection)
            {
                if (selectedAge.worldMusic.world != GameWorlds.ModernDay)
                    currentTables.transform.localPosition -= new Vector3(0f, worldTablesYOffset, 0f);
                else
                    currentTables.transform.localPosition -= new Vector3(0f, modernDayGardenTablesYOffset, 0f);
            }

            RenderSettings.ambientSkyColor = selectedAge.sky;
            RenderSettings.ambientEquatorColor = selectedAge.equator;
            RenderSettings.ambientGroundColor = selectedAge.ground;
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
