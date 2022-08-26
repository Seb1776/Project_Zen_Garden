using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPosition : MonoBehaviour
{
    [SerializeField] private Transform target;
    
    void Update()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player").transform.GetChild(0).GetChild(0).transform;

        transform.position = target.position;
    }
}
