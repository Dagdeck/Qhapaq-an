using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject playerTokenPrefab;
    private GameObject currentPlayerToken;
    private List<GameObject> enemyTokens = new List<GameObject>();
    private HexGridGenerator hexGridGenerator;

    private List<Tile> currentPath = new List<Tile>();
    private Dictionary<GameObject, Tile> currentTiles = new Dictionary<GameObject, Tile>();
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
    private void Start()
    {
        hexGridGenerator = FindObjectOfType<HexGridGenerator>();
        if (hexGridGenerator == null)
        {
            Debug.LogError("HexGridGenerator not found!");
        }
    }
    void Update()
    {
        if (currentPlayerToken == null) return;
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
    public void AddEnemyToken(GameObject token)
    {
        enemyTokens.Add(token);
    }
    public void PlayerMoveComplete()
    {
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy turn started.");
        foreach (var enemy in enemyTokens)
        {
            yield return StartCoroutine(enemy.GetComponent<EnemyToken>().MoveTowardsPlayerCoroutine(currentPlayerToken));
        }
        Debug.Log("Enemy turn ended.");
    }
    /*public void MoveEnemiesTowardsPlayer()
    {
        foreach (var enemy in enemyTokens)
        {
            StartCoroutine(MoveEnemyTowardsPlayer(enemy));
        }
    }
    public IEnumerator MoveEnemyTowardsPlayer(GameObject enemy)
    {
        Tile enemyCurrentTile = GetCurrentTile(enemy);
        Tile playerTile = GetCurrentTile(currentPlayerToken); // Assuming a single player for now

        if (enemyCurrentTile == null || playerTile == null)
        {
            Debug.LogError("Current tile or player tile is null.");
            yield break;
        }

        List<Tile> path = enemyCurrentTile.FindPathTo(playerTile);

        if (path.Count > 0)
        {
            yield return StartCoroutine(MoveToTile(enemy, path[0]));
        }
        else
        {
            Debug.LogError("No path found.");
        }
    }*/
    private IEnumerator MoveToTile(GameObject gameObject, Tile newTile)
    {
        float journeyLength = Vector3.Distance(gameObject.transform.position, newTile.transform.position);
        float startTime = Time.time;

        while (Vector3.Distance(gameObject.transform.position, newTile.transform.position) > 0.01f)
        {
            float distCovered = (Time.time - startTime) * 1f; // Adjust speed as needed
            float fracJourney = distCovered / journeyLength;
            gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, newTile.transform.position, fracJourney);
            yield return null;
        }

        gameObject.transform.position = newTile.transform.position; // Ensure final position is exact
        GameManager.Instance.UpdateCurrentTile(gameObject, newTile);
    }

    public Tile GetCurrentTile(GameObject gameObject)
    {
        if (currentTiles.ContainsKey(gameObject))
        {
            return currentTiles[gameObject];
        }
        return null;
    }

    public void UpdateCurrentTile(GameObject gameObject, Tile newTile)
    {
        if (currentTiles.ContainsKey(gameObject))
        {
            currentTiles[gameObject] = newTile;
        }
        else
        {
            currentTiles.Add(gameObject, newTile);
        }
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
        PlayerMoveComplete(); // Notify the GameManager that the player's move is complete
    }
}
