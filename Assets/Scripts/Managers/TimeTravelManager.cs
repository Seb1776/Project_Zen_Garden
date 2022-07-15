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
                selectedAge.props.SetActive(true);
                currentProps = selectedAge.props;
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
}
