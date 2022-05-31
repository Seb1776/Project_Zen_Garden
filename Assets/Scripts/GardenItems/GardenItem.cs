using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GardenItem : MonoBehaviour
{
    [SerializeField] protected bool grabbed;
    [SerializeField] protected GardenItemType itemType;
    protected Vector3 startPos;
    protected bool canUseItem;

    private bool returnToPos;
    private GameManager manager;
    private Collider coll;
    protected Player player;

    public virtual void Awake()
    {   
        coll = GetComponent<Collider>();

        manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        
        startPos = transform.position;
    }

    public virtual void Start() {}

    public virtual void Update()
    {
        LookInCameraDirection();

        if (!grabbed && Vector3.Distance(transform.position, startPos) > 0.1f)
        {
            grabbed = true;
            player.SetGardenItem(this);
        }
        
        if (grabbed && !returnToPos && (transform.localRotation.x == 0f && transform.localRotation.z == 0f))
        {
            returnToPos = true;
            player.DeSetGardenItem();
        }
        
        if (returnToPos)
            GetBackPos();
    }

    public virtual void GardenItemAction(InputAction.CallbackContext ctx) {}

    void GetBackPos()
    {
        transform.position = Vector3.Lerp(transform.position, startPos, 2.5f * Time.deltaTime);
        coll.enabled = false;

        if (Vector3.Distance(transform.position, startPos) <= 0.001f)
        {
            transform.position = startPos;
            grabbed = false;
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
