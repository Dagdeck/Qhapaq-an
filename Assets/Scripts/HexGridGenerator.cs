using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    public GameObject hexPrefab;    // Hexagon prefab
    public GameObject playerSpawnPrefab; // Player spawn token prefab
    public GameObject specialHexPrefab; // Special hexagon prefab
    public GameObject token; // Player token prefab
    public GameObject enemyPrefab; // Enemy prefab

    public int gridSize = 8;        // Number of rings of hexagons to generate
    public float generationDelay = 0.1f; // Delay between generating each hexagon
    public float hexdistancex = 1.15f;
    public float hexdistancez = 0.575f;
    public Vector3 playerOffset = new Vector3(0, 0.5f, 0); // Offset for player spawn position

    public Dictionary<Vector3, GameObject> hexGrid = new Dictionary<Vector3, GameObject>();
    private List<Vector3> outermostRingPositions = new List<Vector3>();
    private List<Vector3> cornerPositions = new List<Vector3>();
    public Vector3 enemyOffset = new Vector3(0, 0.5f, 0); // Offset for enemy spawn position

    void Start()
    {
        // Start the coroutine to generate the hexagon grid with a delay
        StartCoroutine(GenerateHexagon());
    }

    IEnumerator GenerateHexagon()
    {
        yield return StartCoroutine(InstantiateHexagon(Vector3.zero, 0, 0));

        // Generate subsequent rings
        for (int depth = 1; depth <= gridSize; depth++)
        {
            List<Vector3> newHexPositions = new List<Vector3>();

            foreach (var hex in hexGrid.Keys)
            {
                Vector3[] directions = new Vector3[]
                {
                    new Vector3(hexdistancex, 0, 0),                 // Right
                    new Vector3(hexdistancez, 0, 1),                // Top-right
                    new Vector3(-hexdistancez, 0, 1),               // Top-left
                    new Vector3(-hexdistancex, 0, 0),                // Left
                    new Vector3(-hexdistancez, 0, -1),              // Bottom-left
                    new Vector3(hexdistancez, 0, -1)                // Bottom-right
                };

                foreach (var direction in directions)
                {
                    Vector3 newPosition = hex + direction;
                    if (!hexGrid.ContainsKey(newPosition) && !newHexPositions.Contains(newPosition))
                    {
                        newHexPositions.Add(newPosition);
                    }
                }
            }
            outermostRingPositions.Clear();
            foreach (var newPosition in newHexPositions)
            {
                yield return StartCoroutine(InstantiateHexagon(newPosition, 0, depth));
                outermostRingPositions.Add(newPosition); // Track positions of the outermost ring
            }
            DetermineCornerPositions();
        }
        if (outermostRingPositions.Count > 0)
        {
            Vector3 playerSpawnPosition = cornerPositions[Random.Range(0, cornerPositions.Count)] + playerOffset;
            token = Instantiate(playerSpawnPrefab, playerSpawnPosition, Quaternion.identity);
            Tile initialTile = GetNearestTile(token.transform.position);
            token.GetComponent<PlayerToken>().currentTile = initialTile;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetCurrentPlayerToken(token);
                Debug.Log("Player token set in GameManager.");
            }
            else
            {
                Debug.LogError("GameManager instance is null.");
            }
        }
        ReplaceRandomHexagons(10); // Replace 10 random hexagons
        SpawnEnemies(5);
    }

    IEnumerator InstantiateHexagon(Vector3 position, int q, int r)
    {
        // Check if the position is already occupied by a hexagon using collider check
        Collider[] colliders = Physics.OverlapSphere(position, 0.5f); // Adjust radius as needed
        if (colliders.Length > 0)
        {
            yield break; // Exit if there's already a hexagon at this position
        }

        GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity);
        hex.transform.SetParent(this.transform); // Set parent to keep hierarchy clean
        hexGrid[position] = hex; // Add the position to the dictionary

        // Ensure the Tile component is attached to the hexagon prefab
        Tile tile = hex.GetComponent<Tile>();
        if (tile == null)
        {
            tile = hex.AddComponent<Tile>();
        }

        // Wait for the specified delay
        yield return new WaitForSeconds(generationDelay);
    }

    void DetermineCornerPositions()
    {
        cornerPositions.Clear();

        // Define the 6 corners based on the outermost ring of the hexagon grid
        float outerRadius = gridSize * hexdistancex;
        Vector3[] directions = new Vector3[]
        {
            //new Vector3(outerRadius, 0, 0),                                 // Right
            new Vector3(0.5f * outerRadius, 0, Mathf.Sqrt(3) / 2 * outerRadius), // Top-right
            new Vector3(-0.5f * outerRadius, 0, Mathf.Sqrt(3) / 2 * outerRadius), // Top-left
            //new Vector3(-outerRadius, 0, 0),                                // Left
            new Vector3(-0.5f * outerRadius, 0, -Mathf.Sqrt(3) / 2 * outerRadius), // Bottom-left
            new Vector3(0.5f * outerRadius, 0, -Mathf.Sqrt(3) / 2 * outerRadius)  // Bottom-right
        };

        foreach (var direction in directions)
        {
            Vector3 nearestHexPosition = FindNearestHexPosition(direction);
            if (nearestHexPosition != Vector3.zero)
            {
                cornerPositions.Add(nearestHexPosition);
            }
        }
    }

    Vector3 FindNearestHexPosition(Vector3 position)
    {
        Vector3 nearestPosition = Vector3.zero;
        float minDistance = float.MaxValue;

        foreach (var hexPosition in hexGrid.Keys)
        {
            float distance = Vector3.Distance(position, hexPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPosition = hexPosition;
            }
        }

        return nearestPosition;
    }

    Tile GetNearestTile(Vector3 position)
    {
        Tile nearestTile = null;
        float minDistance = float.MaxValue;

        foreach (var hexPos in hexGrid.Keys)
        {
            float distance = Vector3.Distance(position, hexPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTile = hexGrid[hexPos].GetComponent<Tile>();
            }
        }

        if (nearestTile != null)
        {
            Debug.Log("Nearest tile to position " + position + " is at " + nearestTile.transform.position);
        }
        else
        {
            Debug.LogError("No nearest tile found for position " + position);
        }

        return nearestTile;
    }


    void ReplaceRandomHexagons(int numberOfHexagons)
    {
        // Filter hexagons to include only those within the 8th ring or lower
        List<Vector3> validHexPositions = new List<Vector3>();
        foreach (var hexPos in hexGrid.Keys)
        {
            if (Mathf.Abs(hexPos.x) + Mathf.Abs(hexPos.z) <= 8) // Adjust this condition as needed
            {
                validHexPositions.Add(hexPos);
            }
        }

        // Ensure there are enough hexagons to replace
        numberOfHexagons = Mathf.Min(numberOfHexagons, validHexPositions.Count);

        for (int i = 0; i < numberOfHexagons; i++)
        {
            // Choose a random valid hexagon to replace
            Vector3 randomKey = validHexPositions[Random.Range(0, validHexPositions.Count)];
            validHexPositions.Remove(randomKey); // Remove the chosen hexagon to avoid duplicates

            GameObject oldHex = hexGrid[randomKey];

            // Destroy the old hexagon
            Destroy(oldHex);

            // Instantiate the special hexagon prefab at the same position
            GameObject specialHex = Instantiate(specialHexPrefab, randomKey, Quaternion.identity);
            specialHex.transform.SetParent(this.transform); // Set parent to keep hierarchy clean
            hexGrid[randomKey] = specialHex; // Replace the hexagon in the dictionary
        }
    }

    void SpawnEnemies(int numberOfEnemies)
    {
        List<Vector3> availablePositions = new List<Vector3>(hexGrid.Keys);
        availablePositions.RemoveAll(pos => cornerPositions.Contains(pos));

        for (int i = 0; i < numberOfEnemies && availablePositions.Count > 0; i++)
        {
            Vector3 spawnPosition = availablePositions[Random.Range(0, availablePositions.Count)] + enemyOffset;
            Debug.Log("Attempting to spawn enemy at: " + spawnPosition);

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // Attempt to find the nearest tile to the enemy's spawn position
            Tile enemyInitialTile = GetNearestTile(spawnPosition);
            if (enemyInitialTile != null)
            {
                enemy.GetComponent<EnemyToken>().currentTile = enemyInitialTile;
                Debug.Log("Assigned enemy to tile at: " + enemyInitialTile.transform.position);
            }
            else
            {
                Debug.LogError("Failed to find nearest tile for enemy spawn at " + spawnPosition);
            }

            GameManager.Instance.AddEnemyToken(enemy);
            availablePositions.Remove(spawnPosition - enemyOffset);
        }
    }
}
