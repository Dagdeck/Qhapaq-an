using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGridGenerator : MonoBehaviour
{
    public GameObject hexPrefab;
    public GameObject playerSpawnPrefab;
    public GameObject[] specialHexPrefabs; // Array of special hexagon prefabs
    public GameObject token;
    public GameObject[] enemyPrefab;

    public int gridSize = 8;
    public float generationDelay = 0.1f;
    public float hexdistancex = 1.15f;
    public float hexdistancez = 0.575f;
    public Vector3 playerOffset = new Vector3(0, 0.5f, 0);
    public Vector3 enemyOffset = new Vector3(0, 0.5f, 0);

    public Dictionary<Vector3, GameObject> hexGrid = new Dictionary<Vector3, GameObject>();
    private List<Vector3> outermostRingPositions = new List<Vector3>();
    private List<Vector3> cornerPositions = new List<Vector3>();
    private HashSet<Vector3> specialHexPositions = new HashSet<Vector3>();


    public AudioSource GenerateHexagonAudioSource;
    public AudioClip GenerateHexagonAudioClip;

    void Start()
    {
        
        StartCoroutine(GenerateHexagon());
    }

    IEnumerator GenerateHexagon()
    {
        yield return StartCoroutine(InstantiateHexagon(Vector3.zero));

        for (int depth = 1; depth <= gridSize; depth++)
        {
            List<Vector3> newHexPositions = new List<Vector3>();

            foreach (var hex in hexGrid.Keys)
            {
                Vector3[] directions = new Vector3[]
                {
                    new Vector3(hexdistancex, 0, 0),
                    new Vector3(hexdistancez, 0, 1),
                    new Vector3(-hexdistancez, 0, 1),
                    new Vector3(-hexdistancex, 0, 0),
                    new Vector3(-hexdistancez, 0, -1),
                    new Vector3(hexdistancez, 0, -1)
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
                yield return StartCoroutine(InstantiateHexagon(newPosition));
                outermostRingPositions.Add(newPosition);
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

        ReplaceRandomHexagons(50);
        SpawnEnemies(5);
    }

    IEnumerator InstantiateHexagon(Vector3 position)
    {
         
        
        Collider[] colliders = Physics.OverlapSphere(position, 0.5f);
        if (colliders.Length > 0)
        {
            
            yield break;
        }

        GameObject hex = Instantiate(hexPrefab, position, Quaternion.identity);
        hex.transform.SetParent(this.transform);

        // si el sonido no se está reproduciendo, entonces reproduce el sonido
        if (!GenerateHexagonAudioSource.isPlaying)
        {
        GenerateHexagonAudioSource.PlayOneShot(GenerateHexagonAudioClip);
        }
        hexGrid[position] = hex;
         
        Tile tile = hex.GetComponent<Tile>() ?? hex.AddComponent<Tile>();
         
        yield return new WaitForSeconds(generationDelay);
    }

    void DetermineCornerPositions()
    {
        cornerPositions.Clear();
        float outerRadius = gridSize * hexdistancex;
        Vector3[] directions = new Vector3[]
        {
            new Vector3(0.5f * outerRadius, 0, Mathf.Sqrt(3) / 2 * outerRadius),
            new Vector3(-0.5f * outerRadius, 0, Mathf.Sqrt(3) / 2 * outerRadius),
            new Vector3(-0.5f * outerRadius, 0, -Mathf.Sqrt(3) / 2 * outerRadius),
            new Vector3(0.5f * outerRadius, 0, -Mathf.Sqrt(3) / 2 * outerRadius)
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
        List<Vector3> validHexPositions = new List<Vector3>();
        foreach (var hexPos in hexGrid.Keys)
        {
            if (Mathf.Abs(hexPos.x) + Mathf.Abs(hexPos.z) <= 10)
            {
                validHexPositions.Add(hexPos);
            }
        }

        numberOfHexagons = Mathf.Min(numberOfHexagons, validHexPositions.Count);

        for (int i = 0; i < numberOfHexagons; i++)
        {
            Vector3 randomKey = validHexPositions[Random.Range(0, validHexPositions.Count)];
            validHexPositions.Remove(randomKey);

            GameObject oldHex = hexGrid[randomKey];
            Destroy(oldHex);

            GameObject specialHex = specialHexPrefabs[Random.Range(0, specialHexPrefabs.Length)];
            GameObject instantiatedSpecialHex = Instantiate(specialHex, randomKey, Quaternion.identity);
            instantiatedSpecialHex.transform.SetParent(this.transform);

            Tile tile = instantiatedSpecialHex.GetComponent<Tile>();
            if (tile == null)
            {
                tile = instantiatedSpecialHex.AddComponent<Tile>(); // Ensure Tile component is added
            }
            tile.isSpecialTile = true; // Set the property

            hexGrid[randomKey] = instantiatedSpecialHex;
            specialHexPositions.Add(randomKey);
        }
    }
    public void ReplaceHexagonWithSpecial(Vector3 position)
    {
        if (hexGrid.ContainsKey(position))
        {
            GameObject oldHex = hexGrid[position];
            Destroy(oldHex);

            // Select a random special hex prefab from the array
            GameObject specialHex = specialHexPrefabs[Random.Range(0, specialHexPrefabs.Length)];
            GameObject instantiatedSpecialHex = Instantiate(specialHex, position, Quaternion.identity);
            instantiatedSpecialHex.transform.SetParent(this.transform);
            hexGrid[position] = instantiatedSpecialHex;
            specialHexPositions.Add(position); // Add position to special hex positions set

            Tile tile = instantiatedSpecialHex.GetComponent<Tile>();
            if (tile != null)
            {
                tile.SetSpecialTile(true);
            }
        }
    }

    void SpawnEnemies(int numberOfEnemies)
    {
        List<Vector3> availablePositions = new List<Vector3>(hexGrid.Keys);
        availablePositions.RemoveAll(pos => cornerPositions.Contains(pos));
        availablePositions.RemoveAll(pos => specialHexPositions.Contains(pos)); // Exclude special hex positions

        int enemiesSpawned = 0;
        List<Vector3> validSpawnPositions = new List<Vector3>();

        foreach (var pos in availablePositions)
        {
            Vector3 spawnPosition = pos + enemyOffset;
            int distance = Mathf.Abs((int)spawnPosition.x) + Mathf.Abs((int)spawnPosition.z);

            if (distance <= 7)
            {
                validSpawnPositions.Add(spawnPosition);
            }
        }

        ShuffleList(validSpawnPositions);

        for (int i = 0; i < Mathf.Min(numberOfEnemies, validSpawnPositions.Count); i++)
        {
            Vector3 spawnPosition = validSpawnPositions[i];
            Debug.Log("Attempting to spawn enemy at: " + spawnPosition);

            GameObject enemy = Instantiate(enemyPrefab[Random.Range(0,3)], spawnPosition, Quaternion.identity);
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
            enemiesSpawned++;

            if (enemiesSpawned >= numberOfEnemies)
            {
                break;
            }
        }
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[randomIndex];
            list[randomIndex] = list[i];
            list[i] = temp;
        }
    }
    public void ReplaceSpecialHexWithNormalHex(Vector3 position)
    {
        if (specialHexPositions.Contains(position))
        {
            GameObject specialHex = hexGrid[position];
            Destroy(specialHex);

            GameObject normalHex = Instantiate(hexPrefab, position, Quaternion.identity);
            normalHex.transform.SetParent(this.transform);

            hexGrid[position] = normalHex;
            specialHexPositions.Remove(position);
        }
        else
        {
            Debug.LogError("Position is not a special hex: " + position);
        }
    }
}
