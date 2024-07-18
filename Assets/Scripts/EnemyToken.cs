using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyToken : MonoBehaviour
{
    public Tile currentTile;

    public IEnumerator MoveTowardsPlayerCoroutine(GameObject playerToken)
    {
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
                int moveDistance = 1;
                if (gameObject.CompareTag("Enemy1"))
                {
                    moveDistance = 1;
                }
                else if (gameObject.CompareTag("Enemy2"))
                {
                    moveDistance = 2;
                }
                else if (gameObject.CompareTag("Enemy3"))
                {
                    moveDistance = 3;
                }
                else
                {
                    Debug.LogError("Enemy tag not recognized.");
                }

                // Ensure the move distance does not exceed the path length
                moveDistance = Mathf.Min(moveDistance, path.Count - 1);

                // Find a valid tile to move to
                for (int i = 1; i <= moveDistance; i++)
                {
                    Tile newTile = path[i];
                    if (!GameManager.Instance.IsTileOccupied(newTile))
                    {
                        yield return StartCoroutine(MoveToTile(newTile));
                        moved = true;
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
}
