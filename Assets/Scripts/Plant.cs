using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    //[SerializeField] private Transform plantPot;

    private GameManager manager;
    [SerializeField] private bool planted;
    private Collider coll;
    private Transform handPosition;
    private Player player;

    void Start()
    {
        coll = GetComponent<Collider>();
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        if (!planted)
            TransparentPlant();
    }

    void Update()
    {
        LookInCameraDirection();

        if (!planted && handPosition != null)
            transform.position = handPosition.position;
    }

    public void SetHandPosition(Transform _hand)
    {
        handPosition = _hand;
    }

    void TransparentPlant()
    {
        foreach (SpriteRenderer sr in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            Color transp = new Color(1f, 1f, 1f, .5f);
            sr.color = transp;
        }
    }

    public void ApplyColorToPlant()
    {
       foreach (SpriteRenderer sr in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            Color transp = new Color(1f, 1f, 1f, 1f);
            sr.color = transp;
        } 
    }

    public void SetPlanted()
    {
        coll.enabled = false;
        planted = true;
    }

    void LookInCameraDirection()
    {
        transform.LookAt(manager.GetMainCamera().transform);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f));
    }

    void OnTriggerEnter(Collider c)
    {
        if (c.transform.CompareTag("FlowerPot"))
            if (c.GetComponent<FlowerPot>() != null && c.isTrigger)
                player.HoveringAFlowerPot(c.GetComponent<FlowerPot>());
    }

    void OnTriggerExit(Collider c)
    {
        if (c.transform.CompareTag("FlowerPot"))
            if (c.GetComponent<FlowerPot>() != null && c.isTrigger)
                player.HoveringAFlowerPot(null);
    }
}
