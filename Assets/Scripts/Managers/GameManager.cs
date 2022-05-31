using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;

    public Camera GetMainCamera()
    {
        return mainCam;
    }
}
