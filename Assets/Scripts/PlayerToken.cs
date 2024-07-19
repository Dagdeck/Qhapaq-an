using UnityEngine;

public class PlayerToken : MonoBehaviour
{
    public Tile currentTile; // Reference to the current tile
    public bool hasShield = false;

    public void ActivateShield()
    {
        hasShield = true;
        Debug.Log("Shield activated!");
    }
    public bool HasShield()
    {
        return hasShield;
    }
    public void RemoveShield()
    {
        hasShield = false;
        Debug.Log("Shield removed!");
    }
    private void Update()
    {
        CheckForVictory();
    }

    private void CheckForVictory()
    {
        if (currentTile != null && currentTile.isVictoryTile)
        {
            Debug.Log("Player reached a victory tile! You win!");
            GameManager.Instance.Victory();
        }
    }
}

