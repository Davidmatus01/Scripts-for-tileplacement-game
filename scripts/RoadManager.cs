using System.Collections.Generic;
using UnityEngine;

public static class RoadManager
{
    private static Dictionary<Vector3, RoadFeature> tileToRoadMap = new();
    private static HashSet<RoadFeature> activeRoads = new();
    public static HashSet<RoadFeature> ActiveRoads => activeRoads;

    private static readonly Dictionary<Direction, Vector3> DirectionOffsets = new()
    {
        { Direction.North, new Vector3(0, 0, -1) },
        { Direction.East,  new Vector3(-1, 0, 0) },
        { Direction.South, new Vector3(0, 0, 1) },
        { Direction.West,  new Vector3(1, 0, 0) }
    };

    public static RoadFeature TraverseRoad(Vector3 startPos, float gridSize)
    {
        var toVisit = new Queue<Vector3>();
        var visited = new HashSet<Vector3>();

        RoadFeature road = new RoadFeature();

        //Debug.Log($"[RoadManager] Starting road traversal at {startPos}");

        toVisit.Enqueue(startPos);

        while (toVisit.Count > 0)
        {
            Vector3 current = toVisit.Dequeue();
            if (visited.Contains(current)) continue;
            visited.Add(current);

            //Debug.Log($"[RoadManager] Visiting tile at {current}");

            road.roadLenght++;

            if (!tileToRoadMap.TryGetValue(current, out RoadFeature roadAtTile))
            {
                road.AddTile(current);
                tileToRoadMap[current] = road;
            }
            else if (roadAtTile != road)
            {
                //Debug.Log($"[RoadManager] Merging road at {current}");
                road = MergeRoads(road, roadAtTile);
            }

            GameObject tileObj = GridManager.Instance.GetTileAtPosition(current);
            TileData tileData = tileObj?.GetComponent<TileData>();
            if (tileData == null)
            {
                //Debug.LogWarning($"[RoadManager] No TileData at {current}");
                continue;
            }

            foreach (var edge in tileData.edges)
            {
                if (edge.type != EdgeType.Road) continue;

                Vector3 offset = DirectionOffsets[edge.direction] * gridSize;
                Vector3 neighborPos = current + offset;

                GameObject neighborObj = GridManager.Instance.GetTileAtPosition(neighborPos);
                TileData neighborData = neighborObj?.GetComponent<TileData>();

                if (neighborData == null)
                {
                    Debug.Log($"[RoadManager] Open connection (no neighbor) at {neighborPos}");
                    road.openConnections++;
                    continue;
                }

                Direction oppositeDir = GetOppositeDirection(edge.direction);
                Edge neighborEdge = neighborData.edges.Find(e => e.direction == oppositeDir && e.type == EdgeType.Road);

                if (neighborEdge == null)
                {
                    //Debug.Log($"[RoadManager] Open connection (mismatched neighbor) at {neighborPos}");
                    road.openConnections++;
                    continue;
                }

                //Debug.Log($"[RoadManager] Queuing neighbor at {neighborPos}");

                var neighborEdges = neighborData.edges.FindAll(e => e.type == EdgeType.Road);
                if (neighborEdges.Count > 2)
                {
                    //Debug.Log($"[RoadManager] Endpoint/junction detected at {neighborPos}");
                    road.roadLenght++;
                    if (!road.tiles.Contains(neighborPos))
                    {
                        road.tiles.Add(neighborPos);
                    }
                }
                else
                {
                    toVisit.Enqueue(neighborPos);
                } 
            }

        }

        if(road.openConnections == 0)
        {
             road.setCompleteChecked(true);
        }
      
        //Debug.Log($"[RoadManager] Finished road traversal. Length: {road.roadLenght}, Open connections: {road.openConnections}, Complete: {road.isComplete}");
        if ( road.isComplete)
        {
            if(road.MeepleCount.Count == 0)
            {
                if (GameManager.instance.currentPlayer.TakeMeeple())
                {
                    road.AddMeeple(GameManager.instance.currentPlayer);
                    Debug.Log($"Auto-placed meeple on completed road for scoring.");
                }
            }
            ScoreManager.ScoreRoad(road);
            
            UnregisterRoad(road);
            NextTurnHandler.instance.NextTurn();
        }
        else
        {
            RegisterRoad(road);
        }

        return road;
    }

    private static RoadFeature MergeRoads(RoadFeature roadA, RoadFeature roadB)
    {
        if (roadA == roadB) return roadA;

        foreach (var tile in roadB.tiles)
        {
            if (!roadA.tiles.Contains(tile))
            {
                roadA.AddTile(tile);
                tileToRoadMap[tile] = roadA;
            }
        }
        foreach (var kvp in roadB.MeepleCount)
        {
            if (!roadA.MeepleCount.ContainsKey(kvp.Key))
                roadA.MeepleCount[kvp.Key] = 0;
            roadA.MeepleCount[kvp.Key] += kvp.Value;
        }
        roadB.tiles.Clear();

        UnregisterRoad(roadB);

        if (!roadA.isComplete)
            RegisterRoad(roadA);
        else
            UnregisterRoad(roadA);

        return roadA;
    }

    public static void HandleJunction(Vector3 junctionPos, float gridSize)
    {
        GameObject tileObj = GridManager.Instance.GetTileAtPosition(junctionPos);
        TileData tileData = tileObj?.GetComponent<TileData>();
        if (tileData == null) return;

        HashSet<RoadFeature> alreadyProcessed = new();

        foreach (var edge in tileData.edges)
        {
            if (edge.type != EdgeType.Road) continue;

            Vector3 offset = DirectionOffsets[edge.direction] * gridSize;
            Vector3 neighborPos = junctionPos + offset;

            GameObject neighborObj = GridManager.Instance.GetTileAtPosition(neighborPos);
            TileData neighborData = neighborObj?.GetComponent<TileData>();
            if (neighborData == null) continue;

            //neighbor tile has a road edge opposite of this junction edge
            Direction oppositeDir = GetOppositeDirection(edge.direction);
            Edge neighborEdge = neighborData.edges.Find(e => e.direction == oppositeDir && e.type == EdgeType.Road);
            if (neighborEdge == null) continue;

            var neighborRoadEdges = neighborData.edges.FindAll(e => e.type == EdgeType.Road);
            if (neighborRoadEdges.Count > 2)
            {
                if(neighborEdge.type != EdgeType.Road) continue;
                //Debug.Log($"[RoadManager] Found junction-to-junction between {junctionPos} and {neighborPos}");
                //Debug.Log($"[RoadManager] Completed short road between junctions!");
                ScoreManager.AddPoints(2, "Junctions completed short road", GameManager.instance.currentPlayer);
                continue;
            }

            // if neighbor is already in a road we've handled, skip
            if (tileToRoadMap.TryGetValue(neighborPos, out RoadFeature road) && alreadyProcessed.Contains(road))
            {
                //Debug.Log($"[RoadManager] Skipping already processed road at {neighborPos}");
                continue;
            }

            // otherwise, traverse this road segment
            //Debug.Log($"[RoadManager] Traversing road from junction towards {neighborPos}");
            RoadFeature traversedRoad = TraverseRoad(neighborPos, gridSize);

            alreadyProcessed.Add(traversedRoad);
            if (traversedRoad.isComplete)
            {
                //Debug.Log($"[RoadManager] Completed road segment from junction! Size: {traversedRoad.roadLenght}");
            }
        }
    }

    public static void RegisterRoad(RoadFeature road)
    {
        if (!road.isComplete) activeRoads.Add(road);
    }

    public static void UnregisterRoad(RoadFeature road)
    {
        activeRoads.Remove(road);
    }


    private static Direction GetOppositeDirection(Direction dir)
    {
        return (Direction)(((int)dir + 2) % 4);
    }
}
