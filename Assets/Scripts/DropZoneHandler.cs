using UnityEngine;
using UnityEngine.EventSystems;

public class DropZoneHandler : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            CardDragHandler card = eventData.pointerDrag.GetComponent<CardDragHandler>();
            if (card != null)
            {
                card.UseCard();
            }
        }
    }
}

