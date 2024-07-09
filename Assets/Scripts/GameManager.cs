using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject playerTokenPrefab;
    private GameObject currentPlayerToken;

    private List<Tile> currentPath = new List<Tile>();
    public bool isPathBuilding = false;
    private int maxPathLength = 0;

    public bool IsPathBuilding
    {
        get { return isPathBuilding; }
        private set { isPathBuilding = value; }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetMaxPathLength(int length)
    {
        maxPathLength = length;
        Debug.Log("Max path length set to: " + maxPathLength);
    }
    public void SetCurrentPlayerToken(GameObject token)
    {
        currentPlayerToken = token;
    }

    public void StartPathBuilding()
    {
        isPathBuilding = true;
        currentPath.Clear();
    }

    public void AddTileToPath(Tile tile)
    {
        if (currentPath.Count < maxPathLength)
        {
            if (currentPath.Count == 0)
            {
                Tile currentTile = currentPlayerToken.GetComponent<PlayerToken>().currentTile;
                if (currentTile.IsAdjacent(tile))
                {
                    currentPath.Add(tile);
                    tile.Select();
                    Debug.Log($"Starting path with tile: {tile.name} at position {tile.transform.position}");
                }
                else
                {
                    Debug.LogWarning("Selected tile is not adjacent to the current tile.");
                }
            }
            else
            {
                Tile lastTile = currentPath[currentPath.Count - 1];
                if (lastTile.IsAdjacent(tile))
                {
                    currentPath.Add(tile);
                    tile.Select();
                    Debug.Log($"Added tile to path: {tile.name} at position {tile.transform.position}");
                }
                else
                {
                    Debug.LogWarning("Selected tile is not adjacent to the last tile in the path.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Maximum path length reached.");
            return;
        }
    }

    public void EndPathBuilding()
    {
        isPathBuilding = false;
        MovePlayerAlongPath();
    }

    public void MovePlayerAlongPath()
    {
        StartCoroutine(MovePlayerToken());
    }

    private IEnumerator MovePlayerToken()
    {
        foreach (Tile tile in currentPath)
        {
            Vector3 startPosition = currentPlayerToken.transform.position;
            Vector3 endPosition = tile.transform.position + Vector3.up * 0.5f; // Assuming player token height offset

            float travelTime = 0.5f;
            float elapsedTime = 0;

            while (elapsedTime < travelTime)
            {
                currentPlayerToken.transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / travelTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            currentPlayerToken.transform.position = endPosition;
            //Reset the color of the tile
            tile.ResetColor();
            currentPlayerToken.GetComponent<PlayerToken>().currentTile = tile;
        }
        // Clear the path after movement is complete
        currentPath.Clear();
        //Reset the path building mode
        isPathBuilding = false;
    }
}
