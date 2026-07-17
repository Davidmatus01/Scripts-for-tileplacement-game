using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private GameManager gameManager;
    public static UIManager instance;
    

    void Start()
    {
        instance = this;
        gameManager = GameManager.instance;
        GameManager.RegisterUI(this);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (gameManager == null || gameManager.currentPlayer == null)
            return;

        scoreText.text = $"{gameManager.currentPlayer.playerName}'s Score: {gameManager.currentPlayer.GetPoints()}, Meeples: {gameManager.currentPlayer.availableMeeples}";
    }
}