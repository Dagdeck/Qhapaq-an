using UnityEngine;
using UnityEngine.EventSystems;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPosition;
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GameObject.FindObjectOfType<Canvas>();

        if (rectTransform == null)
        {
            Debug.LogError("RectTransform is not attached to the GameObject.");
        }
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is not attached to the GameObject.");
        }
        if (canvas == null)
        {
            Debug.LogError("Canvas is not found in the parent hierarchy.");
        }
    }

    public void InitializeCard()
    {
        startPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f; // Make the card semi-transparent during drag
        canvasGroup.blocksRaycasts = false; // Ensure other elements can be interacted with
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out originalPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, canvas.worldCamera, out Vector2 localPoint))
        {
            rectTransform.anchoredPosition += (Vector2)eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (!RectTransformUtility.RectangleContainsScreenPoint(GameObject.Find("Drop Zone").GetComponent<RectTransform>(), eventData.position, canvas.worldCamera))
        {
            rectTransform.anchoredPosition = startPosition;
        }
        else
        {
            UseCard();
        }
    }

    public void UseCard()
    {
        Debug.Log("Card used!");
        rectTransform.anchoredPosition = startPosition;
        GameManager.Instance.isPathBuilding = true;
        if (gameObject.CompareTag("Card1"))
        {
            GameManager.Instance.SetMaxPathLength(1);
        }
        else if (gameObject.CompareTag("Card2"))
        {
            GameManager.Instance.SetMaxPathLength(2);
        }
        else if (gameObject.CompareTag("Card3"))
        {
            GameManager.Instance.SetMaxPathLength(3);
        }
        else
        {
            Debug.LogError("Card tag not recognized.");
        }
        CardDrawer.Instance.UseCard(gameObject);
    }
}



