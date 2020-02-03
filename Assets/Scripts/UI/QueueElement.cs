using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class QueueElement : MonoBehaviour, IDragHandler
{
    [SerializeField] bool Active = true;

    public void OnDrag(PointerEventData eventData)
    {
        if (Active)
        {
            var underElement = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<QueueElement>();
            if (underElement != null)
                transform.SetSiblingIndex(eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<QueueElement>().transform.GetSiblingIndex());
        }
    }

}
