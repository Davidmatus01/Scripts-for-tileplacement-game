using System.Collections.Generic;
using UnityEngine;

public class CityFeature : IFeature
{
    public HashSet<Vector3> tiles = new();
    public int openConnections = 0;
    public int citySize = 0;
    public bool isComplete => isCompleteChecked;
    public Dictionary<Player, int> MeepleCount { get; private set; } = new Dictionary<Player, int>();

    private bool isCompleteChecked = false;

    public void AddTile(Vector3 position)
    {
        tiles.Add(position);
    }
    public int TileCount()
    {
        return tiles.Count;
    }

    public void SetCompleteChecked(bool status)
    {
        isCompleteChecked = status;
    }

    public void AddMeeple(Player player)
    {
        if (!MeepleCount.ContainsKey(player))
            MeepleCount[player] = 0;
        MeepleCount[player]++;
    }
}
