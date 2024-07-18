using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public GameObject playerToken; // Assign the player token in the Inspector
    public GameManager gameManager; // Reference to the GameManager script
    public KeyCode startKey = KeyCode.Space; // Key to start token movement
    public GameObject card;
    private int selectedCardValue = 0;

    void Start()
    {

    }

    void Update()
    {
        // Check for the start key press
        if (Input.GetKeyDown(startKey))
        {
            ConfirmPath();
        }
    }

    // Start token movement along the path after path building is complete
    public void ConfirmPath()
    {
        GameManager.Instance.MovePlayerAlongPath();
    }
}