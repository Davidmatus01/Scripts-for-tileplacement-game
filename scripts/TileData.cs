using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TileData : MonoBehaviour
{
    [SerializeField]
    public List<Edge> edges = new List<Edge>();
    public bool isChurch = false;
    public bool isSplitTile = false;
    public bool hasCrest = false;
    public int spawnCount = 1; //instances of object to be spawned and enqued
    public bool isStartingTile = false;
    public Player churchOwner;

    public void LogEdges() //logs into console information about edges of tile 
    {
        Debug.Log("Logging edges: ");
        foreach (var edge in edges)
        {
            Debug.Log(edge.direction + ": " + edge.type);

        }
    }

    public static Direction RotateDirection(Direction dir, int rotation90Steps) //rotates direction of edge +1 for clockwise -1 counterclockwise
    {
        int dirCount = System.Enum.GetValues(typeof(Direction)).Length;
        int newDir = ((int)dir + rotation90Steps) % dirCount;
        if (newDir < 0) newDir += dirCount;
        return (Direction)newDir;
    }

    public void RotateTileEdges(int rotationStepsClockwise) //rotates all directions for Tile
    {
        foreach (var edge in edges)
        {
            edge.direction = RotateDirection(edge.direction, rotationStepsClockwise);
        }
    }

    public EdgeType GetEdgeType(Direction dir) //returns Edge type for inserted direction
    {
        return edges.Find(e => e.direction == dir).type;
    }

    public static bool IsCityTile(TileData data) //checks if tile has city edge
    {
        return data.edges.Any(e => e.type == EdgeType.City);
    }

    public static bool IsRoadTile(TileData data) //checks if tile has road edge
    {
        return data.edges.Any(e => e.type == EdgeType.Road);
    }
}
