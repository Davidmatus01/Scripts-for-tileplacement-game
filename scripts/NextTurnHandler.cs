using UnityEngine;
using UnityEngine.UI;

public class NextTurnHandler : MonoBehaviour
{
    [SerializeField] private Button nextTurnButton;
    private TilePlacer tilePlacer;
    private GameManager gameManager;
    public static NextTurnHandler instance;

    void Start()
    {
        gameManager = GameManager.instance;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void NextTurn()
    {
        tilePlacer = PlacementQueueManager.Instance.current;
        if (tilePlacer == null) return;
        if (tilePlacer.getIsPlaced())
        {
            PlacementQueueManager.Instance.NotifyTilePlaced(tilePlacer);
            gameManager.NextPlayer();
        }
        else
        {
            SimpleMessageUI.instance.Show($"You must play first before ending your turn.");
            Debug.Log("You must play first before ending your turn.");
        }
        Debug.Log(tilePlacer.name);
        Debug.Log("Next turn called by button");
    }
}
