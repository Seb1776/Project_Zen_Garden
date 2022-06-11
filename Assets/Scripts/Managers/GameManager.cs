using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;

    private Player player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
}
