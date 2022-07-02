using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class GardenItem : MonoBehaviour
{
    [SerializeField] private string soundEffect;
    [SerializeField] protected bool grabbed;
    [SerializeField] protected GardenItemType itemType;
    protected Vector3 startPos;
    protected bool canUseItem;
    protected bool isUsable;

    [Header ("Raycast Fields, Leave Empty If Not Using")]
    [SerializeField] protected LayerMask rayMask;
    [SerializeField] protected bool showGizmo;
    [SerializeField] protected Vector3 rayDirection;
    [SerializeField] protected Transform rayPos;
    [SerializeField] protected float rayLength;

    private SortingGroup sg;
    protected XRGrabInteractable grab;
    protected FlowerPot detectedPot;
    private bool returnToPos;
    private GameManager manager;
    private Collider coll;
    protected Player player;

    public virtual void Awake()
    {   
        coll = GetComponent<Collider>();
        grab = GetComponent<XRGrabInteractable>();
        sg = GetComponent<SortingGroup>();

        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        
        startPos = transform.position;
    }

    public virtual void Start() {}

    public virtual void DetectEffect() {}

    public virtual void Update()
    {   
        grabbed = grab.isSelected;

        LookInCameraDirection();

        if (grabbed)
            player.SetGardenItem(this);
        
        else
        {
            player.DeSetGardenItem(this);
            
            if (Vector3.Distance(transform.position, startPos) > 0.1f)
                returnToPos = true;
        }

        if (returnToPos)
            GetBackPos();
    }

    public virtual void GardenItemAction(InputAction.CallbackContext ctx) {}

    protected FlowerPot GetBelowFlowerPot()
    {
        RaycastHit hit;

        if (Physics.Raycast(rayPos.position, rayPos.transform.TransformDirection(Vector3.forward), out hit, rayLength, rayMask))
        {
            if (hit.collider != null)
            {
                if (hit.transform.CompareTag("FlowerPot"))
                {
                    return hit.transform.GetComponent<FlowerPot>();
                }
            }
        }

        return null;
    }

    protected void GardenItemSFX()
    {
        StartCoroutine(SoundEffectsManager.instance.PlaySoundEffect(soundEffect));
    }

    void OnDrawGizmos()
    {
        if (showGizmo && rayPos != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(rayPos.position, rayPos.transform.TransformDirection(Vector3.forward) * rayLength);
        }
    }

    void GetBackPos()
    {
        transform.position = Vector3.Lerp(transform.position, startPos, 2.5f * Time.deltaTime);
        coll.enabled = false;

        if (Vector3.Distance(transform.position, startPos) <= 0.001f)
        {
            transform.position = startPos;
            returnToPos = false;
            coll.enabled = true;
        }
    }

    void LookInCameraDirection()
    {
        transform.LookAt(manager.GetMainCamera().transform);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f));
    }
}
