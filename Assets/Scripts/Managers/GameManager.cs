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
    public bool unlockedModernDay;
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

    public GameWorlds GetGameWorldFromString(string world)
    {
        switch (world)
        {
            case "AncientEgypt": return GameWorlds.AncientEgypt;
            case "PirateSeas": return GameWorlds.PirateSeas;
            case "WildWest": return GameWorlds.WildWest;
            case "FarFuture": return GameWorlds.FarFuture;
            case "DarkAges": return GameWorlds.DarkAges;
            case "BigWaveBeach": return GameWorlds.BigWaveBeach;
            case "FrostbiteCaves": return GameWorlds.FrostbiteCaves;
            case "LostCity": return GameWorlds.LostCity;
            case "NeonMixtapeTour": return GameWorlds.NeonMixtapeTour;
            case "JurassicMarsh": return GameWorlds.JurassicMarsh;
            case "ModernDay": return GameWorlds.ModernDay;
            case "Tutorial": return GameWorlds.Tutorial;
        }

        return GameWorlds.ThrowbackToThePresent;
    }
}

public static class GameHelper
{
    public static float GetPercentageFromValue(float value, float perc)
    {
        return (perc / value) * 100f;
    }

    public static bool GetBoolFromChance(float chance)
    {
        if (chance >= 100f)
            return true;
        
        if (chance <= 0f)
            return false;

        float percVal = chance / 100f;
        float randVal = Random.value;
        return percVal >= randVal;
    }

    public static bool GetRandomBool()
    {
        return Random.value >= .5f;
    }
}
