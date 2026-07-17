using UnityEngine;
using UnityEngine.UI;

public class TileDiscarder : MonoBehaviour
{
    [SerializeField] private Button discardButton;  
    private TilePlacer tilePlacer;

    public void DiscardCurrentTile()
    {
        tilePlacer = PlacementQueueManager.Instance.current;
        if (tilePlacer == null) return;
        Debug.Log(tilePlacer.name);
        Debug.Log("Tile discard called by button");
        tilePlacer.DiscardTile();  
    }
}
