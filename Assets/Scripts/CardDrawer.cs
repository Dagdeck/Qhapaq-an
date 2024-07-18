using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CardDrawer : MonoBehaviour
{
    public static CardDrawer Instance;

    public List<GameObject> cardPrefab; // Prefab for the card
    public Transform[] cardSpawnPoints; // The spawn point where cards should initially appear
    public Transform cardContainer;  // The container where the cards should be parented
    public Vector2 cardOffset = new Vector2(25f, 0f); // Offset between cards
    public int maxCardsToDraw = 3; // Maximum number of cards to draw initially

    private List<GameObject> drawnCards = new List<GameObject>();
    private bool[] spawnPointsOccupied;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        spawnPointsOccupied = new bool[cardSpawnPoints.Length];
    }

    void Start()
    {
        DrawCards(maxCardsToDraw);
        Debug.Log("Cards drawn: " + drawnCards.Count);
    }

    public void DrawCards(int numberOfCards)
    {
        for (int i = 0; i < numberOfCards; i++)
        {
            DrawSingleCard();
        }
    }

    public void DrawSingleCard()
    {
        int spawnPointIndex = GetFreeSpawnPoint();
        if (spawnPointIndex == -1)
        {
            Debug.LogError("No free spawn points available.");
            return;
        }

        Transform spawnPoint = cardSpawnPoints[spawnPointIndex];
        GameObject newCard = Instantiate(cardPrefab[Random.Range(0, cardPrefab.Count)], spawnPoint.position, Quaternion.identity, cardContainer);
        newCard.transform.SetParent(cardContainer, false);
        RectTransform rectTransform = newCard.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = spawnPoint.GetComponent<RectTransform>().anchoredPosition;

        drawnCards.Add(newCard);
        spawnPointsOccupied[spawnPointIndex] = true;
        newCard.GetComponent<CardDragHandler>().InitializeCard();
    }

    public void UseCard(GameObject card)
    {
        StartCoroutine(UseCardCoroutine(card));
    }
    private IEnumerator UseCardCoroutine(GameObject card)
    {
        yield return new WaitForEndOfFrame(); // Wait until the end of the frame to ensure card destruction is handled correctly

        if (drawnCards.Contains(card))
        {
            int cardIndex = drawnCards.IndexOf(card);
            Debug.Log("Card index is:" +cardIndex);
            MarkSpawnPointFree(card); // Mark the corresponding spawn point as free
            Destroy(card);
            DrawSingleCard(); // Draw a new card immediately after using one
        }
    }
    private void MarkSpawnPointFree(GameObject cardToRemove)
    {
        // Check if the card exists in drawnCards
        if (drawnCards.Contains(cardToRemove))
        {
            // Find the card's position before removing it
            Vector3 cardPosition = cardToRemove.transform.position;

            // Remove the card from the drawnCards list
            drawnCards.Remove(cardToRemove);

            // Now find and mark the closest spawn point as free based on card's position
            int closestSpawnPointIndex = FindClosestSpawnPointIndex(cardPosition);

            if (closestSpawnPointIndex != -1)
            {
                spawnPointsOccupied[closestSpawnPointIndex] = false;
            }
            else
            {
                Debug.LogError("No valid spawn point found for card position: " + cardPosition);
            }
        }
        else
        {
            Debug.LogError("Trying to mark spawn point free for a card that is not in drawnCards.");
        }
    }

    private int FindClosestSpawnPointIndex(Vector3 cardPosition)
    {
        int closestIndex = -1;
        float closestDistance = float.MaxValue;

        // Iterate through all spawn points to find the closest one to cardPosition
        for (int i = 0; i < cardSpawnPoints.Length; i++)
        {
            float distance = Vector3.Distance(cardSpawnPoints[i].position, cardPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }




    private int GetFreeSpawnPoint()
    {
        for (int i = 0; i < spawnPointsOccupied.Length; i++)
        {
            if (!spawnPointsOccupied[i])
            {
                return i;
            }
        }
        return -1; // No free spawn point found
    }

    private int GetSpawnPointIndex(Vector3 position)
    {
        for (int i = 0; i < cardSpawnPoints.Length; i++)
        {
            if (Vector3.Distance(cardSpawnPoints[i].position, position) < 0.1f)
            {
                return i;
            }
        }
        return -1; // Spawn point not found
    }
}






