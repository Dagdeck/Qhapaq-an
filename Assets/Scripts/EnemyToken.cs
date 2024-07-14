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

        List<Tile> path = enemyCurrentTile.FindPathTo(playerTile);

        if (path.Count > 0)
        {
            Tile nextTile = path[0];
            yield return StartCoroutine(MoveToTile(nextTile));
        }
        else
        {
            Debug.LogError("No path found.");
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



