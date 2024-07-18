using System.Collections.Generic;
using Unity.VisualScripting;
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
        if (gameObject.CompareTag("Card1"))
        {
            GameManager.Instance.isPathBuilding = true;
            GameManager.Instance.SetMaxPathLength(1);
            CardDrawer.Instance.UseCard(gameObject);
        }
        else if (gameObject.CompareTag("Card2"))
        {
            GameManager.Instance.isPathBuilding = true;
            GameManager.Instance.SetMaxPathLength(2);
            CardDrawer.Instance.UseCard(gameObject);
        }
        else if (gameObject.CompareTag("Card3"))
        {
            GameManager.Instance.isPathBuilding = true;
            GameManager.Instance.SetMaxPathLength(3);
            CardDrawer.Instance.UseCard(gameObject);
        }
        else if (gameObject.CompareTag("Viento"))
        {
            CardDrawer.Instance.UseCard(gameObject);
        }
        else if (gameObject.CompareTag("Luna"))
        {
            FreezeRandomEnemy(2);
            CardDrawer.Instance.UseCard(gameObject);
        }
        else if (gameObject.CompareTag("Terremoto"))
        {
            GameManager.Instance.StartReplacingTile();
            CardDrawer.Instance.UseCard(gameObject);
        }
        else if (gameObject.CompareTag("Sol"))
        {
            GameManager.Instance.currentPlayerToken.GetComponent<PlayerToken>().ActivateShield();
            CardDrawer.Instance.UseCard(gameObject);
        }
        else
        {
            Debug.LogError("Card tag not recognized.");
        }
    }
    private void FreezeRandomEnemy(int turns)
    {
        var enemyTags = new string[] { "Enemy1", "Enemy2", "Enemy3" }; // Add all enemy tags here
        var enemies = new List<GameObject>();

        foreach (var tag in enemyTags)
        {
            enemies.AddRange(GameObject.FindGameObjectsWithTag(tag));
        }

        if (enemies.Count > 0)
        {
            var enemyToFreeze = enemies[Random.Range(0, enemies.Count)];
            enemyToFreeze.GetComponent<EnemyToken>().FreezeForTurns(turns);
        }
    }
}



