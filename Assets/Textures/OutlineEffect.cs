using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OutlineEffect : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private float outlineScaleFactor;
    [SerializeField] private Color outlineColor;
    public Renderer outlineRenderer;
    private GameObject previouslyCreatedOutline;

    void Start()
    {
        outlineRenderer = CreateOutline(outlineMaterial, outlineScaleFactor, outlineColor);
        outlineRenderer.enabled = false;
    }

    public void ChangeOutlineColor(Color _outlineColor, bool activate)
    {   
        if (activate)
            previouslyCreatedOutline.GetComponent<Renderer>().material.SetColor("_OutlineColor", _outlineColor);

        outlineRenderer.enabled = activate;
    }

    Renderer CreateOutline(Material outlineMat, float scaleFactor, Color color)
    {
        previouslyCreatedOutline = Instantiate(this.gameObject, transform.position, transform.rotation, transform);
        Renderer rend = previouslyCreatedOutline.GetComponent<Renderer>();

        rend.material = outlineMat;
        rend.material.SetColor("_OutlineColor", color);
        rend.material.SetFloat("_Scale", scaleFactor);
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Destroy(previouslyCreatedOutline.GetComponent<OutlineEffect>());

        rend.enabled = false;

        return rend;
    }
}