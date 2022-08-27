using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public string gardenName;
    public float spentTime;
    public bool startCounting;
    public GameObject gameCanvas, gardenCanvas;
    [SerializeField] private Camera mainCam;
    [Header ("Environment")]
    [SerializeField] private float skySpeed;
    [Header ("Tutorial Stuff")]
    public bool onTutorial;
    [SerializeField] private GameObject jurassicMarshBlocker;
    [SerializeField] private GameObject tutorial;

    private Player player;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", skySpeed * Time.time);
        
        if (startCounting)
            spentTime += Time.deltaTime;
    }

    public float GetSpentTime()
    {
        return spentTime;
    }

    public void FirstTimeTutorial()
    {
        onTutorial = true;
        TriggerTutorial(true);
    }

    public Camera GetMainCamera()
    {
        return mainCam;
    }

    public void TriggerTutorial(bool activate)
    {
        tutorial.SetActive(activate);
    }
}

public static class GameHelper
{
    public static float GetPercentageFromValue(float value, float perc)
    {
        return (perc / value) * 100f;
    }

    public static bool GetRandomBool()
    {
        return Random.value >= .5f;
    }
}
