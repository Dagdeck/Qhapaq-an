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
            if (collider.gameObject == otherTile.gameObject)
            {
                return true;
            }
        }
        return false;
    }
}

