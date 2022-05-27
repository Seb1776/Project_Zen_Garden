using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Plant : MonoBehaviour
{
    [SerializeField] private PlantAsset plantData;
    [SerializeField] private AnimationClip sproutDeAppearAnimation;

    private GameManager manager;
    [SerializeField] private bool planted;
    private bool growth;
    private GameObject _sprout;
    private GameObject _plant;
    private Collider coll;
    private Transform handPosition;
    private Player player;
    private Animator plantAnim;
    private Vector3 plantFullSize;

    void Awake()
    {
        plantAnim = GetComponent<Animator>();
        coll = GetComponent<Collider>();
        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        plantAnim.speed = 0f;
    }

    void Start()
    {   
        if (!planted)
            TransparentPlant();
    }

    void Update()
    {
        LookInCameraDirection();

        if (!planted && handPosition != null)
            transform.position = handPosition.position;
        
        if (Keyboard.current.kKey.wasPressedThisFrame)
            GrowPlant();
        
        if (growth)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, plantData.initialScale, 2 * Time.deltaTime);

            if (Vector3.Distance(transform.localScale, plantData.initialScale) <= 0.001f)
            {
                transform.localScale = plantData.initialScale;
                growth = false;
            }
        }
    }

    public void SetHandPosition(Transform _hand)
    {
        handPosition = _hand;
    }

    void GrowPlant()
    {   
        if (!growth)
        {
            _sprout.GetComponent<Animator>().SetTrigger("hide");
            StartCoroutine(GrowPlantAfterSprout());
        }
    }

    IEnumerator GrowPlantAfterSprout()
    {
        yield return new WaitForSeconds(sproutDeAppearAnimation.length);
        Destroy(_sprout.gameObject);
        growth = true;
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

        transform.localScale = Vector3.zero;
        plantAnim.speed = 1f;
    }

    public void SetPlanted(GameObject _sp)
    {
        _sprout = _sp;
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
