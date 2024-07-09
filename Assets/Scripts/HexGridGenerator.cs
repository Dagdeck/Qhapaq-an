using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    public GameObject hexPrefab;    // Hexagon prefab
    public GameObject playerSpawnPrefab; // Player spawn token prefab
    public GameObject specialHexPrefab; // Special hexagon prefab
    public GameObject token; // Player token prefab

    public int gridSize = 2;        // Number of rings of hexagons to generate
    public float generationDelay = 0.1f; // Delay between generating each hexagon
    public float hexdistancex = 1.15f;
    public float hexdistancez = 0.575f;
    public Vector3 playerOffset = new Vector3(0, 0.5f, 0); // Offset for player spawn position

    private Dictionary<Vector3, GameObject> hexGrid = new Dictionary<Vector3, GameObject>();
    private List<Vector3> outermostRingPositions = new List<Vector3>();

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
        }
        if (outermostRingPositions.Count > 0)
        {
            Vector3 playerSpawnPosition = outermostRingPositions[Random.Range(0, outermostRingPositions.Count)] + playerOffset;
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
        ReplaceRandomHexagon();
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

        // Wait for the specified delay
        yield return new WaitForSeconds(generationDelay);
    }

    Tile GetNearestTile(Vector3 position)
    {
        Tile nearestTile = null;
        float minDistance = float.MaxValue;

        foreach (var hex in hexGrid.Values)
        {
            float distance = Vector3.Distance(position, hex.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTile = hex.GetComponent<Tile>();
            }
        }
        return nearestTile;
    }

    void ReplaceRandomHexagon()
    {
        // Choose a random hexagon to replace
        Vector3 randomKey = new List<Vector3>(hexGrid.Keys)[Random.Range(0, hexGrid.Count)];
        GameObject oldHex = hexGrid[randomKey];

        // Destroy the old hexagon
        Destroy(oldHex);

        // Instantiate the special hexagon prefab at the same position
        GameObject specialHex = Instantiate(specialHexPrefab, randomKey, Quaternion.identity);
        specialHex.transform.SetParent(this.transform); // Set parent to keep hierarchy clean
        hexGrid[randomKey] = specialHex; // Replace the hexagon in the dictionary
    }
}

