using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Color defaultColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color pathColor = Color.green;

    private Renderer tileRenderer;

    void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        ResetColor();
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
        if (GameManager.Instance != null && GameManager.Instance.IsPathBuilding)
        {
            GameManager.Instance.AddTileToPath(this);
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
}

