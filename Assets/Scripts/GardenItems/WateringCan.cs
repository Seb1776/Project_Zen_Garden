using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WateringCan : GardenItem
{
    [SerializeField] private Transform rayPos;
    [SerializeField] private float rayLength;
    [SerializeField] private Vector2 acceptableRotationRange;
    [SerializeField] private LayerMask rayMask;
    [SerializeField] private bool showGizmo;
    [SerializeField] private ParticleSystem waterParticles;

    public override void Awake()
    {
        base.Awake();
    }

    public override void Update()
    {
        if (transform.eulerAngles.z >= acceptableRotationRange.x && transform.eulerAngles.z <= acceptableRotationRange.y)
            canUseItem = true;
        
        else
            canUseItem = false;

        base.Update();
    }

    public override void GardenItemAction(InputAction.CallbackContext ctx)
    {
        if (canUseItem)
        {
            waterParticles.Play();
        }

        else
        {
            waterParticles.Pause();
        }

        base.GardenItemAction(ctx);
    }

    void OnDrawGizmosSelected()
    {
        if (showGizmo && rayPos != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(rayPos.position, transform.TransformDirection(Vector3.left) * rayLength);
        }
    }
}
