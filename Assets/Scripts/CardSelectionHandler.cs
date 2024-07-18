using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class CardSelectionHandler : MonoBehaviour, IPointerClickHandler
{
    public event Action<GameObject> OnCardSelected;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardSelected?.Invoke(gameObject);
    }
}

