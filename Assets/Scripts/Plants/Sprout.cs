using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sprout : MonoBehaviour
{
    private GameManager manager;

    void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        LookInCameraDirection();
    }

    void LookInCameraDirection()
    {
        transform.LookAt(manager.GetMainCamera().transform);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f));
    }
}
