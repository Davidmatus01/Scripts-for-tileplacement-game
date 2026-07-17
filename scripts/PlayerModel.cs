using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string playerName;
    private int score = 0;
    public int availableMeeples = 7;

    public Player(string name, int meepleCount = 7)
    {
        playerName = name;

    }

    public bool TakeMeeple()
    {
        if (availableMeeples > 0)
        {
            availableMeeples--;
            return true;
        }
        return false;
    }

    public void ReturnMeeple(int count) {
        
            availableMeeples += count; 
    }

    public void ScoreIncrease(int points)
    {
        score += points;
    }

    public int GetPoints() => score;

    public bool HasAvailableMeeples() => availableMeeples > 0;
}