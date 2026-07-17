using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public Transform gridOrigin;
    public float gridSize = 30f;
    public int width = 10;
    public int height = 10;
    public Color gridColor = Color.green;

    private HashSet<Vector3> occupiedPositions = new HashSet<Vector3>();
    private Dictionary<Vector3, GameObject> placedTiles = new Dictionary<Vector3, GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public Vector3 GetSnappedPosition(Vector3 originalPosition) //calculates snapped position used for grid and tile management
    {
        float x = Mathf.Round(originalPosition.x / gridSize) * gridSize;
        float z = Mathf.Round(originalPosition.z / gridSize) * gridSize;
        return new Vector3(x, 0f, z);
    }

    public bool IsPositionOccupied(Vector3 position)
    {
        return occupiedPositions.Contains(position);
    }

    public void OccupyPosition(Vector3 position, GameObject tile) //occupies position with tile at given vector
    {
        if (!occupiedPositions.Contains(position))
        {
            occupiedPositions.Add(position);
            placedTiles[position] = tile;
        }
    }

    public GameObject GetTileAtPosition(Vector3 position) //returns tile at inserted position
    {
        placedTiles.TryGetValue(position, out GameObject tile);
        return tile;
    }

    public void RemoveTileAtPosition(Vector3 position)
    {
        if (occupiedPositions.Contains(position))
        {
            occupiedPositions.Remove(position);
            placedTiles.Remove(position);
        }
    }

    public IReadOnlyDictionary<Vector3, GameObject> GetAllPlacedTiles()
    {
        return placedTiles;
    }

    private void OnDrawGizmos() //draws visible grid in Edit mode only
    {
        if (gridOrigin == null) return;

        Gizmos.color = gridColor;
        Vector3 origin = gridOrigin.position;

        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * gridSize, 0, 0);
            Vector3 end = origin + new Vector3(x * gridSize, 0, height * gridSize);
            Gizmos.DrawLine(start, end);
        }

        for (int z = 0; z <= height; z++)
        {
            Vector3 start = origin + new Vector3(0, 0, z * gridSize);
            Vector3 end = origin + new Vector3(width * gridSize, 0, z * gridSize);
            Gizmos.DrawLine(start, end);
        }
    }

    public Vector3 GetDirectionOffset(Direction dir)
    {
        return dir switch
        {
            Direction.North => new Vector3(0, 0, GridManager.Instance.gridSize),
            Direction.East => new Vector3(GridManager.Instance.gridSize, 0, 0),
            Direction.South => new Vector3(0, 0, -GridManager.Instance.gridSize),
            Direction.West => new Vector3(-GridManager.Instance.gridSize, 0, 0),
            _ => Vector3.zero
        };
    }
}
