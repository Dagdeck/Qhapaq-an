using System.Collections.Generic;
using UnityEngine;

public class CardDeckManager : MonoBehaviour
{
    public static CardDeckManager Instance;

    public List<GameObject> cardPrefabs; // List of card prefabs
    private List<GameObject> deck = new List<GameObject>();

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

        InitializeDeck();
    }

    // Initialize the deck with card prefabs
    private void InitializeDeck()
    {
        foreach (var cardPrefab in cardPrefabs)
        {
            deck.Add(cardPrefab);
        }

        // Optionally shuffle the deck
        ShuffleDeck();
    }

    // Shuffle the deck
    private void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            GameObject temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // Draw a card from the deck
    public GameObject DrawCard()
    {
        if (deck.Count == 0)
        {
            Debug.LogError("Deck is empty!");
            return null;
        }

        GameObject drawnCard = deck[Random.Range(0,deck.Count)];
        return drawnCard;
    }
}


