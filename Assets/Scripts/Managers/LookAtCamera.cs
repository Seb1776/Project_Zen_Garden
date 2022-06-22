using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private GameManager manager;
    private Vector3 initRotation; 

    void Awake()
    {
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        initRotation = transform.eulerAngles;
    }

    void Update()
    {
        transform.LookAt(manager.GetMainCamera().transform);
        transform.rotation = Quaternion.Euler(new Vector3(initRotation.x, transform.eulerAngles.y, initRotation.z));
    }
}
