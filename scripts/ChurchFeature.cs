using System.Collections.Generic;
using UnityEngine;

public class ChurchFeature : IFeature
{
    public Vector3 position;
    public Dictionary<Player, int> MeepleCount { get; private set; } = new Dictionary<Player, int>();
    public bool isComplete => false;

    public ChurchFeature(Vector3 pos)
    {
        position = pos;
    }

    public void AddMeeple(Player player)
    {
        if (!MeepleCount.ContainsKey(player))
            MeepleCount[player] = 0;
        MeepleCount[player]++;
    }
}