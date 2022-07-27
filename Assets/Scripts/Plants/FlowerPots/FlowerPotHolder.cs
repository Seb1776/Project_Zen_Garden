using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerPotHolder : MonoBehaviour
{   
    public bool canShowEffect;
    public Transform gardenParent;

    public OutlineEffect outline;

    void Start()
    {
        outline = transform.GetChild(0).GetComponent<OutlineEffect>();
        gardenParent = transform.parent;
    }

    void OnTriggerStay(Collider other) 
    {
        if (canShowEffect)
        {
            if (other.CompareTag("FlowerPot") && other.transform.GetComponent<FlowerPot>().GetFPHolder() == null)
            {
                outline.ChangeOutlineColor(Color.green, true);
                other.transform.GetComponent<FlowerPot>().hoveringHolder = this;

                if (other.transform.GetComponent<FlowerPot>().setted)
                    other.transform.GetComponent<FlowerPot>().reAssignable = true;
            }
        }
    }

    void OnTriggerExit(Collider other) 
    {
        if (canShowEffect)
        {
            if (other.CompareTag("FlowerPot"))
            {
                outline.ChangeOutlineColor(Color.green, false);
                other.transform.GetComponent<FlowerPot>().hoveringHolder = null;

                if (other.transform.GetComponent<FlowerPot>().setted)
                    other.transform.GetComponent<FlowerPot>().reAssignable = false;
            }
        }
    }
}
