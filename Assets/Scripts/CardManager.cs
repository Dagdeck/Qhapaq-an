using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    public GameObject playerToken; // Assign the player token in the Inspector
    public GameManager gameManager; // Reference to the GameManager script
    public KeyCode startKey = KeyCode.Space; // Key to start token movement
    public Button card2;
    public Button card3;
    public GameObject card;
    private int selectedCardValue = 0;

    void Start()
    {
        //If a button is clicked, the OnCardSelected method is called with the corresponding card value
        card2.onClick.AddListener(() => OnCardSelected(2));
        card3.onClick.AddListener(() => OnCardSelected(3));
    }

    void Update()
    {
        //If the card is clicked, the OnCardSelected method is called with the corresponding card value
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Card"))
                {
                    OnCardSelected(1);
                }
            }
        }
        // Check for the start key press
        if (Input.GetKeyDown(startKey))
        {
            ConfirmPath();
        }
    }

    public void OnCardSelected(int cardValue)
    {
        Debug.Log("Card selected: " + cardValue);
        selectedCardValue = cardValue;
        GameManager.Instance.isPathBuilding = true; // Enable path building mode
        GameManager.Instance.SetMaxPathLength(selectedCardValue); // Set the maximum path length
    }

    // Start token movement along the path after path building is complete
    public void ConfirmPath()
    {
        GameManager.Instance.MovePlayerAlongPath();
    }
}