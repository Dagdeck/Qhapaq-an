using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToken : MonoBehaviour
{
    public Tile currentTile;
    public int frozenTurns = 0;
    public PlayerToken PlayerToken { get; set; }

    private Tile previousTile; // To track the previous tile

    private void Start()
    {
        PlayerToken = GameManager.Instance.currentPlayerToken.GetComponent<PlayerToken>();
    }

    public void FreezeForTurns(int turns)
    {
        frozenTurns = turns;
    }

    public IEnumerator MoveTowardsPlayerCoroutine(GameObject playerToken)
    {
        if (frozenTurns > 0)
        {
            frozenTurns--;
            yield break; // Skip movement if frozen
        }

        Tile enemyCurrentTile = currentTile;
        Tile playerTile = playerToken.GetComponent<PlayerToken>().currentTile;

        if (enemyCurrentTile == null || playerTile == null)
        {
            Debug.LogError("Current tile or player tile is null.");
            yield break;
        }

        bool moved = false;
        while (!moved)
        {
            List<Tile> path = enemyCurrentTile.FindPathTo(playerTile);

            if (path != null && path.Count > 1)
            {
                // Move based on enemy type
                int moveDistance = GetMoveDistanceBasedOnTag();

                // Ensure the move distance does not exceed the path length
                moveDistance = Mathf.Min(moveDistance, path.Count - 1);

                // Find a valid tile to move to
                for (int i = 1; i <= moveDistance; i++)
                {
                    Tile newTile = path[i];
                    if (!GameManager.Instance.IsTileOccupied(newTile) && !newTile.isSpecialTile)
                    {
                        previousTile = enemyCurrentTile; // Track previous tile
                        yield return StartCoroutine(MoveToTile(newTile));
                        moved = true;
                        CheckForPlayer();
                    }
                }
            }

            // If no valid move found, break the loop
            if (!moved)
            {
                break;
            }
        }
    }

    private int GetMoveDistanceBasedOnTag()
    {
        if (gameObject.CompareTag("Enemy1"))
        {
            return 1;
        }
        else if (gameObject.CompareTag("Enemy2"))
        {
            return 2;
        }
        else if (gameObject.CompareTag("Enemy3"))
        {
            return 3;
        }
        else
        {
            Debug.LogError("Enemy tag not recognized.");
            return 1; // Default move distance
        }
    }

    private IEnumerator MoveToTile(Tile newTile)
    {
        float journeyLength = Vector3.Distance(transform.position, newTile.transform.position);
        float startTime = Time.time;

        while (Vector3.Distance(transform.position, newTile.transform.position) > 0.01f)
        {
            float distCovered = (Time.time - startTime) * 1f; // Adjust speed as needed
            float fracJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(transform.position, newTile.transform.position, fracJourney);
            yield return null;
        }

        transform.position = newTile.transform.position; // Ensure final position is exact
        currentTile = newTile; // Update the enemy's current tile
        GameManager.Instance.UpdateCurrentTile(gameObject, newTile);
    }

    private void Update()
    {
        
    }

    private void CheckForPlayer()
    {
        Tile playerTile = GameManager.Instance.currentPlayerToken.GetComponent<PlayerToken>().currentTile;
        if (playerTile == currentTile)
        {
            if (PlayerToken.HasShield())
            {
                // Deactivate the shield and move the enemy back to its previous tile
                PlayerToken.RemoveShield();
                StartCoroutine(MoveBackToPreviousTile());
                Debug.Log("Enemy collided with player, but shield negated the effect.");
            }
            else
            {
                // Reset the game
                Debug.Log("Enemy collided with player!");
                GameManager.Instance.ResetGame();
            }
        }
    }

    private IEnumerator MoveBackToPreviousTile()
    {
        if (previousTile != null)
        {
            float journeyLength = Vector3.Distance(transform.position, previousTile.transform.position);
            float startTime = Time.time;

            while (Vector3.Distance(transform.position, previousTile.transform.position) > 0.01f)
            {
                float distCovered = (Time.time - startTime) * 1f; // Adjust speed as needed
                float fracJourney = distCovered / journeyLength;
                transform.position = Vector3.Lerp(transform.position, previousTile.transform.position, fracJourney);
                yield return null;
            }

            transform.position = previousTile.transform.position; // Ensure final position is exact
            currentTile = previousTile; // Update the enemy's current tile
            previousTile = null; // Clear previous tile
        }
        frozenTurns = 1; // Freeze the enemy for one turn after moving back
        StartCoroutine(MoveTowardsPlayerCoroutine(GameManager.Instance.currentPlayerToken));
    }
}

