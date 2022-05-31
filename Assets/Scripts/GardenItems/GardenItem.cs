using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GardenItem : MonoBehaviour
{
    [SerializeField] protected bool grabbed;
    [SerializeField] protected GardenItemType itemType;
    protected Vector3 startPos;
    protected bool canUseItem;
    [SerializeField] private Text uidebug, actionDebug;

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

        if (grabbed)
            player.SetGardenItem(this);
        
        else
        {
            player.DeSetGardenItem();
            Debug.Log("???");
        }

        if (uidebug != null)
            uidebug.text = grabbed.ToString();

        if (!grabbed && Vector3.Distance(transform.position, startPos) > 0.1f)
        {
            grabbed = true;
        }
        
        if (grabbed && (transform.localRotation.x == 0f && transform.localRotation.z == 0f))
        {
            returnToPos = true;
        }

        if (returnToPos)
            GetBackPos();
    }

    public virtual void GardenItemAction(InputAction.CallbackContext ctx) 
    {
        actionDebug.text = "works";
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
            grabbed = false;
        }
    }

    void LookInCameraDirection()
    {
        transform.LookAt(manager.GetMainCamera().transform);
        transform.rotation = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f));
    }
}
