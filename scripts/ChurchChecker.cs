using System.Collections.Generic;
using UnityEngine;

public class ChurchChecker : MonoBehaviour
{
    public static ChurchChecker instance;
    public HashSet<ChurchFeature> activeChurches = new();
    void Awake()
    {
        instance = this;
    }

    private static readonly Vector3[] NeighborOffsets = new Vector3[]
    {
        new Vector3(1, 0, 1), // SW
        new Vector3( 0, 0, 1), // S
        new Vector3( -1, 0, 1), // SE
        new Vector3(1, 0,  0), // W
        new Vector3( -1, 0,  0), // E
        new Vector3(1, 0,  -1), // NW
        new Vector3( 0, 0,  -1), // N
        new Vector3( -1, 0,  -1), // NE
    };

    public static bool IsChurchEncircled(Vector3 churchPosition, float gridSize) //Checks if Church is encircled
    {
        foreach (var offset in NeighborOffsets)
        {
            Vector3 neighborPos = churchPosition + offset * gridSize;
            if (!GridManager.Instance.IsPositionOccupied(neighborPos))
            {
                return false; // At least one neighbor is missing
            }
        }
        return true; // All 8 neighbors are occupied

    }

    public void CheckActiveChurches(float gridSize) //Checks all active churches for encirclement and scores them
    {
        var toRemove = new List<ChurchFeature>();

        foreach (var church in activeChurches)
        {
            if (!ChurchChecker.IsChurchEncircled(church.position, gridSize))
                continue;

            if (church.MeepleCount.Count == 0)
            {
                Debug.Log("Encircled church with no meeples – no score awarded.");
                toRemove.Add(church);
                continue;
            }

            foreach (var kvp in church.MeepleCount)
            {
                ScoreManager.AddPoints(9, "Encircled church", kvp.Key);
                kvp.Key.ReturnMeeple(kvp.Value);
                break; 
            }

            toRemove.Add(church);
        }

        foreach (var church in toRemove)
            activeChurches.Remove(church);
    }

    public void AddChurch(ChurchFeature church)
    {
        activeChurches.Add(church);
    }
}