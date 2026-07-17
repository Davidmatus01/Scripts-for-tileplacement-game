using System.Collections.Generic;
using UnityEngine;

public static class CityManager
{
    // Map each tile position to the city it belongs to
    private static Dictionary<Vector3, CityFeature> tileToCityMap = new();
    private static HashSet<CityFeature> activeCities = new();
    public static HashSet<CityFeature> ActiveCities => activeCities;

    private static readonly Dictionary<Direction, Vector3> DirectionOffsets = new()
    {
        { Direction.North, new Vector3(0, 0, -1) },
        { Direction.East,  new Vector3(-1, 0, 0) },
        { Direction.South, new Vector3(0, 0, 1) },
        { Direction.West,  new Vector3(1, 0, 0) }
    };

    // Traverse city starting from a tile, merging with existing cities
    public static CityFeature TraverseAndMergeCity(Vector3 startPos, float gridSize)
    {
        var toVisit = new Queue<Vector3>();
        var visited = new HashSet<Vector3>();

        CityFeature mergedCity = new CityFeature();

        toVisit.Enqueue(startPos);

        while (toVisit.Count > 0)
        {
            Vector3 current = toVisit.Dequeue();
            if (visited.Contains(current)) continue;
            visited.Add(current);
            mergedCity.citySize++; //increases size of current city

            if (!tileToCityMap.TryGetValue(current, out CityFeature cityAtTile))
            {
                mergedCity.AddTile(current);
                tileToCityMap[current] = mergedCity;
            }
            else if (cityAtTile != mergedCity) //merges cities 
            {
                mergedCity = MergeCities(mergedCity, cityAtTile);
            }

            GameObject tileObj = GridManager.Instance.GetTileAtPosition(current);
            TileData tileData = tileObj?.GetComponent<TileData>();
            if (tileData == null) continue;

            foreach (var edge in tileData.edges) //gets neighboring tiles
            {
                if (edge.type != EdgeType.City) continue;

                Vector3 offset = DirectionOffsets[edge.direction] * gridSize;
                Vector3 neighborPos = current + offset;

                GameObject neighborObj = GridManager.Instance.GetTileAtPosition(neighborPos);
                TileData neighborData = neighborObj?.GetComponent<TileData>();

                if (neighborData == null)
                {
                    mergedCity.openConnections++;
                    continue;
                }

                // If neighbor is a split tile, add it to city size but do not enqueue it to prevent merging
                if (neighborData.isSplitTile)
                {
                    mergedCity.citySize++;
                    continue;
                }

                Direction oppositeDir = GetOppositeDirection(edge.direction);
                Edge neighborEdge = neighborData.edges.Find(e => e.direction == oppositeDir && e.type == EdgeType.City);

                if (neighborEdge == null)
                {
                    mergedCity.openConnections++;
                    continue;
                }

                toVisit.Enqueue(neighborPos); //enqueue neighbor for traversal
            }
        }

        if(mergedCity.openConnections == 0)
        {
            mergedCity.SetCompleteChecked(true);
        }

        //Debug.Log($"Merged city size: {mergedCity.citySize}, open edges: {mergedCity.openConnections}, completed {mergedCity.isComplete}");
        if (mergedCity.isComplete)
        {
            if (mergedCity.MeepleCount.Count == 0)
            {
                if (GameManager.instance.currentPlayer.TakeMeeple()) // Auto-place meeple for scoring if player has meeple available and none are present
                {
                    mergedCity.AddMeeple(GameManager.instance.currentPlayer);
                    Debug.Log($"Auto-placed meeple on completed road for scoring.");
                }
            } 
            ScoreManager.ScoreCity(mergedCity, true);
            UnregisterCity(mergedCity);
            NextTurnHandler.instance.NextTurn();
        }
        else
        {
            RegisterCity(mergedCity);
        }
        return mergedCity;
    }

    // Merge cityB into cityA and update the tileToCityMap accordingly
    private static CityFeature MergeCities(CityFeature cityA, CityFeature cityB)
    {
        if (cityA == cityB) return cityA;

        foreach (var tile in cityB.tiles)
        {
            if (!cityA.tiles.Contains(tile))
            {
                cityA.AddTile(tile);
                tileToCityMap[tile] = cityA;
            }
        }
        foreach (var kvp in cityB.MeepleCount)
        {
            if (!cityA.MeepleCount.ContainsKey(kvp.Key))
                cityA.MeepleCount[kvp.Key] = 0;
            cityA.MeepleCount[kvp.Key] += kvp.Value;
        }

        cityB.tiles.Clear();

        // Remove cityB from active set, since it no longer exists
        UnregisterCity(cityB);

        // cityA remains tracked if incomplete
        if (!cityA.isComplete)
            RegisterCity(cityA);
        else
            UnregisterCity(cityA);

        return cityA;
    }

    //Handes split city tiles by traversing its neighbors separately unless they are part of same city
    public static void HandleSplitCity(Vector3 splitTilePos, float gridSize)
    {
        GameObject tileObj = GridManager.Instance.GetTileAtPosition(splitTilePos);
        TileData tileData = tileObj?.GetComponent<TileData>();
        if (tileData == null) return;

        HashSet<CityFeature> alreadyProcessed = new();

        foreach (var edge in tileData.edges)
        {
            if (edge.type != EdgeType.City) continue;

            Vector3 offset = DirectionOffsets[edge.direction] * gridSize;
            Vector3 neighborPos = splitTilePos + offset;

            GameObject neighborObj = GridManager.Instance.GetTileAtPosition(neighborPos);
            TileData neighborData = neighborObj?.GetComponent<TileData>();
            if (neighborData == null) continue;

            // check that the neighbor actually has a city edge facing back
            Direction oppositeDir = GetOppositeDirection(edge.direction);
            Edge neighborEdge = neighborData.edges.Find(e => e.direction == oppositeDir && e.type == EdgeType.City);

            // check if neighbor is also a split tile and are facing each other — if so score that pair
            if (neighborData.isSplitTile)
            {
                if(neighborEdge.type != EdgeType.City) continue;
                //Debug.Log($"[CityManager] Found split-to-split between {splitTilePos} and {neighborPos}");
                //Debug.Log($"[CityManager] Completed tiny city between split tiles!");
                ScoreManager.AddPoints(4, "Split city tiles made mini city", GameManager.instance.currentPlayer); 
                continue;
            }

            if (neighborEdge == null) continue;

            // if neighbor is already part of a processed city, skip
            if (tileToCityMap.TryGetValue(neighborPos, out CityFeature city) && alreadyProcessed.Contains(city))
            {
                Debug.Log($"[CityManager] Skipping already processed city at {neighborPos}");
                continue;
            }

            // otherwise, traverse this city segment
            //Debug.Log($"[CityManager] Traversing city from split tile towards {neighborPos}");
            CityFeature traversedCity = TraverseAndMergeCity(neighborPos, gridSize);


            alreadyProcessed.Add(traversedCity);

            if (traversedCity.isComplete)
            {
                Debug.Log($"[CityManager] Completed city segment from split tile! Size: {traversedCity.citySize}");
            }
        }
    }


    private static Direction GetOppositeDirection(Direction dir)
    {
        return (Direction)(((int)dir + 2) % 4);
    }

    public static void RegisterCity(CityFeature city)
    {
        if (!city.isComplete) activeCities.Add(city);
    }

    public static void UnregisterCity(CityFeature city)
    {
        activeCities.Remove(city);
    }
}

