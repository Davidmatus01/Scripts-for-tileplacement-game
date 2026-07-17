using System.Collections.Generic;
using UnityEngine;

public static class FeatureFinder
{
    public static List<IFeature> GetFeaturesAtTile(Vector3 tilePos, Player player)
    {
        List<IFeature> result = new();

        foreach (var city in CityManager.ActiveCities) 
        {
            if (city.tiles.Contains(tilePos) && city.MeepleCount.Count == 0){
                result.Add(city);
            }
        }

        foreach (var road in RoadManager.ActiveRoads)
        {
            if (road.tiles.Contains(tilePos) && road.MeepleCount.Count == 0)
            {
                result.Add(road);
            }
        }

        foreach (var church in ChurchChecker.instance.activeChurches)
        {
            if (church.position == tilePos && church.MeepleCount.Count == 0)
            {
                result.Add(church);
            }
        }
        return result;
    }
}
