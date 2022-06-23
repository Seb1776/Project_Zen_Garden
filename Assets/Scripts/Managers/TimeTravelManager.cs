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

    private bool loading, doneLoading;


    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void ChangeScenario(string _world)
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
            selectedAge.props.SetActive(true);
            currentProps = selectedAge.props;
        }

        loading = false;
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
    public GameObject props;
    public Material skyboxMaterial;
}
