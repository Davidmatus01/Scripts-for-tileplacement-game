using System.Collections.Generic;
using UnityEngine;

public interface IFeature
{
    Dictionary<Player, int> MeepleCount { get; }
    bool isComplete { get; }
    void AddMeeple(Player player);
}