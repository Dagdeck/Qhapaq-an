using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    public GameObject hexPrefab;    // Hexagon prefab
    public GameObject specialHexPrefab; // Special hexagon prefab
    public int gridSize = 2;        // Number of rings of hexagons to generate
    public float generationDelay = 0.1f; // Delay between generating each hexagon
    public float hexdistancex = 1.15f;
    public float hexdistancez = 0.575f;

    private Dictionary<Vector3, GameObject> hexGrid = new Dictionary<Vector3, GameObject>();

    void Start()
    {
        // Start the coroutine to generate the hexagon grid with a delay
        StartCoroutine(GenerateHexagon());
    }

    IEnumerator GenerateHexagon()
    {
        yield return StartCoroutine(InstantiateHexagon(Vector3.zero, 0, 0, 0));

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

            foreach (var newPosition in newHexPositions)
            {
                yield return StartCoroutine(InstantiateHexagon(newPosition, 0, 0, depth));
            }
        }
        ReplaceRandomHexagon();
    }

    IEnumerator InstantiateHexagon(Vector3 position, int q, int r, int depth)
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
