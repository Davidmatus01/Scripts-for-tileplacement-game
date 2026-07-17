using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreManager
{
    
    public static void AddPoints(int count, string reason, Player player) // Method for adding points to a player with a reason
    {
        player.ScoreIncrease(count);
        SimpleMessageUI.instance.Show($"{player.playerName} gained {count} points");
        Debug.Log($"{reason} and has scored {count} points");
        Debug.Log($"Current points: {player.GetPoints()}");
    }

    public static void ScoreCity(CityFeature city, bool finished) // Method for scoring a city feature
    {
    if (city.MeepleCount.Count == 0)
    {
        Debug.Log("City scored but no meeples - no score awarded.");
        return;
    }

    int crestCount = 0;
    foreach (var tile in city.tiles)
    {
        GameObject tileObj = GridManager.Instance.GetTileAtPosition(tile);
        TileData tileData = tileObj?.GetComponent<TileData>();
        if (tileData != null && tileData.hasCrest)
            crestCount++;
    }

    int score = finished
        ? city.citySize * 2 + crestCount * 2
        : city.citySize + crestCount;

    int maxMeeples = 0;
    foreach (var kvp in city.MeepleCount)
    {
        kvp.Key.ReturnMeeple(kvp.Value);
        if (kvp.Value > maxMeeples)
        {
            maxMeeples = kvp.Value;
        }
    }

        foreach (var kvp in city.MeepleCount)
    {
        if (kvp.Value == maxMeeples)
        {
            kvp.Key.ScoreIncrease(score);
            Debug.Log($"{kvp.Key.playerName} gains {score} points for city.");
            SimpleMessageUI.instance.Show($"{kvp.Key.playerName} gained {score} points and now has {kvp.Key.availableMeeples} meeples");
            }
    }
}

    public static void ScoreRoad(RoadFeature road) // Method for scoring a road feature
    {
        if (road.MeepleCount.Count == 0)
        {
            Debug.Log("Road scored but no meeples - no score awarded.");
            return;
        }

        int maxMeeples = 0;
        foreach (var kvp in road.MeepleCount)
        {
            kvp.Key.ReturnMeeple(kvp.Value);
            if (kvp.Value > maxMeeples)
            {
                maxMeeples = kvp.Value;
            }
        }
            
        foreach (var kvp in road.MeepleCount)
        {
            if (kvp.Value == maxMeeples)
            {
                kvp.Key.ScoreIncrease(road.TileCount());
                Debug.Log($"{kvp.Key.playerName} gains {road.TileCount()} points for road.");
                SimpleMessageUI.instance.Show($"{kvp.Key.playerName} gained {road.TileCount()} points and now has {kvp.Key.availableMeeples} meeples");
            }
        }
    }

}
