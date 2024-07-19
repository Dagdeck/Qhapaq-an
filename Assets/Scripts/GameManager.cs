using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject playerTokenPrefab;
    public GameObject currentPlayerToken;
    private List<GameObject> enemyTokens = new List<GameObject>();
    private HexGridGenerator hexGridGenerator;
    private Tile selectedTile;
    private bool isReplacingTile = false;
    public bool isActionInProgress = false;

    private List<Tile> currentPath = new List<Tile>();
    private Dictionary<GameObject, Tile> currentTiles = new Dictionary<GameObject, Tile>();
    public bool isPathBuilding = false;
    private int maxPathLength = 0;
    public HashSet<Tile> occupiedTiles = new HashSet<Tile>();

    // AudioSources para los sonidos
    public AudioSource pathSelectAudioSource;
    public AudioSource playerMoveAudioSource;
    public AudioSource enemyMoveAudio;

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
        Tile initialTile = token.GetComponent<EnemyToken>().currentTile;
        if (initialTile != null)
        {
            occupiedTiles.Add(initialTile);
        }
    }

    public void PlayerMoveComplete()
    {
        isActionInProgress = false;
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy turn started.");
        foreach (var enemy in enemyTokens)
        {
            enemyMoveAudio.Play(); // Reproduce el sonido de movimiento del enemigo
            yield return StartCoroutine(enemy.GetComponent<EnemyToken>().MoveTowardsPlayerCoroutine(currentPlayerToken));
            enemyMoveAudio.Stop(); // Detén el sonido de movimiento del enemigo
        }
        Debug.Log("Enemy turn ended.");
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
            occupiedTiles.Remove(currentTiles[gameObject]);
            currentTiles[gameObject] = newTile;
        }
        else
        {
            currentTiles.Add(gameObject, newTile);
        }
        occupiedTiles.Add(newTile);
    }

    public bool IsTileOccupied(Tile tile)
    {
        return occupiedTiles.Contains(tile);
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
                    pathSelectAudioSource.Play(); // Reproduce el sonido de selección
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
                    pathSelectAudioSource.Play(); // Reproduce el sonido de selección
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
        playerMoveAudioSource.Play(); // Reproduce el sonido de movimiento
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
            // Reset the color of the tile
            tile.ResetColor();
            currentPlayerToken.GetComponent<PlayerToken>().currentTile = tile;
        }
        playerMoveAudioSource.Stop(); // Detén el sonido de movimiento
        // Clear the path after movement is complete
        currentPath.Clear();
        // Reset the path building mode
        isPathBuilding = false;
        PlayerMoveComplete(); // Notify the GameManager that the player's move is complete
    }
    public void TileSelected(Tile tile)
    {
        if (selectedTile != null)
        {
            selectedTile.HighlightTile(false); // Unhighlight previously selected tile
        }

        selectedTile = tile;
        selectedTile.HighlightTile(true); // Highlight the selected tile
    }

    public void ReplaceSpecialHexWithNormalHex()
    {
        if (selectedTile != null && selectedTile.isSpecialTile)
        {
            Vector3 position = selectedTile.transform.position;
            hexGridGenerator.ReplaceSpecialHexWithNormalHex(position);
        }
        else
        {
            Debug.LogWarning("No selected or non-special tile.");
        }
    }

    public void OnSpecialTileClicked(Tile tile)
    {
        Debug.Log("Special tile clicked. IsReplacingTile: " + isReplacingTile + " IsSpecialTile: " + tile.isSpecialTile);
        if (isReplacingTile && tile.isSpecialTile)
        {
            hexGridGenerator.ReplaceSpecialHexWithNormalHex(tile.GetPosition());
            isReplacingTile = false;
            Debug.Log("Special tile replaced with normal hex at: " + tile.GetPosition());
        }
    }

    public void StartReplacingTile()
    {
        isReplacingTile = true;
        Debug.Log("Tile replacement mode started. Click on a special tile to replace it.");
    }
    public void ResetGame()
    {
        Debug.Log("Game Over!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
    public void Victory()
    {
        Debug.Log("Congratulations! You have won the game.");
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
