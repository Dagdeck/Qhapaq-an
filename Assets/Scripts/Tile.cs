using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color pathColor = Color.green;
    public Color highlightColor = Color.cyan;

    private Renderer tileRenderer;
    private Vector3 position;

    public bool isSpecialTile = false;

    void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        ResetColor();
    }
    private void Start()
    {
        position = transform.position;
    }

    public void Highlight()
    {
        tileRenderer.material.color = selectedColor;
    }

    public void Select()
    {
        tileRenderer.material.color = pathColor;
    }

    public void ResetColor()
    {
        tileRenderer.material.color = defaultColor;
    }

    void OnMouseDown()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPathBuilding && !isSpecialTile)
        {
            GameManager.Instance.AddTileToPath(this);
        }
        else if (isSpecialTile)
        {
            Debug.Log("Tile clicked at position: " + position + " Is special: " + isSpecialTile);
            GameManager.Instance.OnSpecialTileClicked(this);
        }
    }

    // Method to check adjacency using colliders
    public bool IsAdjacent(Tile otherTile)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.0f); // Adjust radius as needed
        foreach (var collider in colliders)
        {
            Tile tile = collider.GetComponent<Tile>();
            if (tile != null && tile == otherTile)
            {
                return true;
            }
        }
        return false;
    }

    public List<Tile> FindPathTo(Tile targetTile)
    {
        Queue<Tile> queue = new Queue<Tile>();
        Dictionary<Tile, Tile> cameFrom = new Dictionary<Tile, Tile>();
        queue.Enqueue(this);
        cameFrom[this] = null;

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            if (current == targetTile)
            {
                List<Tile> path = new List<Tile>();
                while (current != null)
                {
                    path.Add(current);
                    current = cameFrom[current];
                }
                path.Reverse();
                return path;
            }

            foreach (Tile neighbor in current.GetNeighbors())
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }
        return null;
    }

    public List<Tile> GetNeighbors()
    {
        List<Tile> neighbors = new List<Tile>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, 1.0f); // Adjust radius as needed
        foreach (var collider in colliders)
        {
            Tile tile = collider.GetComponent<Tile>();
            if (tile != null && tile != this)
            {
                neighbors.Add(tile);
            }
        }
        return neighbors;
    }

    public void SetSpecialTile(bool isSpecial)
    {
        isSpecialTile = isSpecial;
        // Change appearance based on whether it's a special tile or not
        if (isSpecial)
        {
            tileRenderer.material.color = Color.red; // Example color for special tile
        }
        else
        {
            tileRenderer.material.color = defaultColor;
        }
    }
    public void HighlightTile(bool highlight)
    {
        if (highlight)
        {
            tileRenderer.material.color = highlightColor;
        }
        else
        {
            tileRenderer.material.color = defaultColor;
        }
    }
    public Vector3 GetPosition()
    {
        return transform.position;
    }
}

