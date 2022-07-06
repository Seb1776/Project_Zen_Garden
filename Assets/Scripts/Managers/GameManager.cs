using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] private Camera mainCam;
    [Header ("Environment")]
    [SerializeField] private float skySpeed;

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
    }

    public Camera GetMainCamera()
    {
        return mainCam;
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
