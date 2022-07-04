using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Pinata : MonoBehaviour
{
    [SerializeField] private PinataAsset pinataData;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private string debugSize;
    [SerializeField] private GameObject pinataExplosion;

    private PinataSizeCategory selectedCategory;
    private int squishes, currentSquishes;
    private XRGrabInteractable grab;
    private Vector3 startPos;
    private bool returnToPos, squishCooldown, canGrab;
    private SpriteRenderer sr;
    private Collider coll;
    private Transform objTo;
    private Animator anim;

    void Awake()
    {
        grab = transform.GetChild(0).GetComponent<XRGrabInteractable>();
        objTo = transform.GetChild(0).transform;
        coll = objTo.GetComponent<Collider>();
        anim = GetComponent<Animator>();
        sr = objTo.GetComponent<SpriteRenderer>();

        worldCanvas = transform.GetChild(1).GetComponent<Canvas>();
        worldCanvas.worldCamera = GameManager.instance.GetMainCamera();
        startPos = objTo.position;
        StartCoroutine(InitialAnimation());
    }

    [System.Obsolete]
    void Start()
    {
        grab.onSelectEnter.AddListener(SendPinataToPlayer);
        grab.onSelectExit.AddListener(UnSendPinataToPlayer);
        SetPinataSize(debugSize);
    }

    void Update()
    {   
        if (canGrab)
        {
            if (!grab.isSelected && Vector3.Distance(objTo.position, startPos) > 0.1f)
                returnToPos = true;

            if (returnToPos)
                GetBackPos();
        }
    }

    public void SetPinataSize(string size)
    {
        switch (size)
        {
            case "s": selectedCategory = PinataSizeCategory.S; break;
            case "m": selectedCategory = PinataSizeCategory.M; break;
            case "l": selectedCategory = PinataSizeCategory.L; break;
            case "xl": selectedCategory = PinataSizeCategory.XL; break;
        }

        for (int i = 0; i < pinataData.sizes.Length; i++)
        {   
            if (pinataData.sizes[i].pinataSize == selectedCategory)
            {
                squishes = Random.Range(pinataData.sizes[i].squishesRange.x, pinataData.sizes[i].squishesRange.y);
                break;
            }
        }
    }

    public void Squish()
    {   
        if (!squishCooldown)
        {
            if (currentSquishes >= squishes)
            {
                coll.enabled = false;
                grab.enabled = false;
                sr.enabled = false;
                Player.instance.RecievePinata(null);
                Instantiate(pinataExplosion, transform.position, Quaternion.identity);
                Debug.Log("Pinata Explodes");
            }

            else
            {
                currentSquishes++;

                if (GameHelper.GetRandomBool()) anim.SetTrigger("squishA");
                else anim.SetTrigger("squishB");
            }
        }
    }

    IEnumerator SquishCooldown()
    {
        squishCooldown = true;
        yield return new WaitForSeconds(.15f);
        squishCooldown = false;
    }
    
    IEnumerator InitialAnimation()
    {
        yield return new WaitForSeconds(0.25f);
        canGrab = true;
    }

    void SendPinataToPlayer(XRBaseInteractor inter)
    {
        Player.instance.RecievePinata(this);
    }

    void UnSendPinataToPlayer(XRBaseInteractor inter)
    {
        Player.instance.RecievePinata(null);
    }

    void GetBackPos()
    {
        objTo.position = Vector3.Lerp(objTo.position, startPos, 2.5f * Time.deltaTime);
        coll.enabled = false;

        if (Vector3.Distance(objTo.position, startPos) <= 0.01f)
        {
            objTo.position = startPos;
            returnToPos = false;

            if (currentSquishes < squishes)
                coll.enabled = true;
        }
    }
}
