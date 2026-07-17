using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player currentPlayer;
    public List<Player> PlayerList = new List<Player>();
    private int currentPlayerIndex = 0;
    public TileData lastPlacedTile;
    public Vector3 lastPlacedTilePos;
    private static UIManager ui;
    private GameControls controls;


    void Awake()
    {
        instance = this;
        InitializePlayers();
        currentPlayer = PlayerList[currentPlayerIndex];
        controls = new GameControls();
    }
    void Update()
    {
        if (controls.TilePlacement.NextTurn.WasPressedThisFrame())
        {
            NextTurnHandler.instance.NextTurn();
        }
    }

    public static void RegisterUI(UIManager scoreUI)
    {
        ui = scoreUI;
        if (ui != null)
            ui.UpdateUI();
    }

    public void NextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % PlayerList.Count;
        currentPlayer = PlayerList[currentPlayerIndex];
        ui.UpdateUI();
        Debug.Log($"Its {currentPlayer.playerName}'s turn");
    }

    private void InitializePlayers()
    {
        PlayerList.Clear();

        PlayerList.Add(new Player("Player 1"));
        PlayerList.Add(new Player("Player 2"));
        PlayerList.Add(new Player("Player 3"));
        PlayerList.Add(new Player("Player 4"));
    }

    public void GameEnd() // Called when the game ends, scores all remaining features and determines the winner
    {
        Debug.Log($"Game Over!");
        foreach (var city in CityManager.ActiveCities)
        {
            ScoreManager.ScoreCity(city, false);
        }
        foreach (var road in RoadManager.ActiveRoads)
        {
            ScoreManager.ScoreRoad(road);
        }
        Player winner = CalculateFinalScores();
        PanelManager.instance.ShowEndPanel($"{winner.playerName} has won with score of: {winner.GetPoints()}!");
    }

    private Player CalculateFinalScores()
    {
        int highestScore = -1;
        Player winner = null;
        foreach (var player in PlayerList)
        {
            if (player.GetPoints() > highestScore)
            {
                highestScore = player.GetPoints();
                winner = player;
            }
        }
        return winner;
    }

    public void SetCurrentTile(TileData tile, Vector3 position) // Gets data of the last placed tile and its position
    {
        lastPlacedTile = tile;
        lastPlacedTilePos = position;
    }
    public Player GetPlayer()
    {
        return currentPlayer;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void OnEnable()
    {
        controls.TilePlacement.Enable();
    }

    void OnDisable()
    {
        controls.TilePlacement.Disable();
    }
}
