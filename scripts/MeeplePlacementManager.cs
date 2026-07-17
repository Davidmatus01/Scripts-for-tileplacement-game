using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeeplePlacementManager : MonoBehaviour
{
    public static MeeplePlacementManager instance;
    GameManager gm => GameManager.instance;
    PanelManager panel => PanelManager.instance;

    void Awake()
    {
        instance = this;
    }

    public void TryPlaceMeeple() //starts meeple placement process
    {
        Debug.Log(gm.currentPlayer.playerName + " is trying to place meeple.");
        Player player = gm.currentPlayer;
        if (!player.HasAvailableMeeples())
        {
            Debug.Log("Player has no meeples.");
            SimpleMessageUI.instance.Show($"{player.playerName} has no meeples");
            NextTurnHandler.instance.NextTurn();
            return;
        }
        if (IsSplitTile()) {
            return;
        }

        Vector3 tilePos = gm.lastPlacedTilePos;

        List<IFeature> features = FeatureFinder.GetFeaturesAtTile(tilePos, player);

        if (features.Count == 0)
        {
            Debug.Log("No valid feature for meeple.");
            SimpleMessageUI.instance.Show($"No place for meeple to be placed");
            NextTurnHandler.instance.NextTurn();
            return;
        }

        Debug.Log($"Found {features.Count} valid features for meeple.");
        panel.ShowFeatures(features);
    }

    public void PlaceMeeple(IFeature feature) //places meeple on selected feature
    {
        Player player = GameManager.instance.currentPlayer;
        
        if (player.TakeMeeple())
        {
            feature.AddMeeple(player);
            MeepleSpawner.instance.SpawnMeeple(feature, gm.lastPlacedTilePos + new Vector3(0, 2f, 0));
            Debug.Log($"Meeple placed on {feature.GetType().Name}");
            SimpleMessageUI.instance.Show($"Meeple placed, {player.playerName} has {player.availableMeeples} meeples left");
            NextTurnHandler.instance.NextTurn();
        }
        else
        {
            Debug.Log("Player has no meeples to place.");
        }
    }

    private bool IsSplitTile()
    {
        TileData tile = gm.lastPlacedTile;
        if (tile == null)
        {
            Debug.Log("No tile data found.");
            return false;
        }
        if(tile.isSplitTile)
        {
            Debug.Log("Cannot place meeple on split tile.");
            NextTurnHandler.instance.NextTurn();
            return true;
        }
        var roadEdgeCount = tile.edges.FindAll(e => e.type == EdgeType.Road);
        if(roadEdgeCount.Count > 2)
        {
            Debug.Log("Cannot place meeple on split road tile.");
            NextTurnHandler.instance.NextTurn();
            return true;
        }

        return false;
    }
}
