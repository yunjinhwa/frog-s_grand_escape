using UnityEngine;

public class GameState
{
    private static readonly GameState instance = new GameState();
    public static GameState Instance => instance;

    private int hp;
    private int score;

    public int HP => hp;
    public int Score => score;

    private GameState()
    {
        hp = 3;      // 시작 체력
        score = 0;
    }

    public void DecreaseHP(int amount = 1)
    {
        hp -= amount;
        if (hp < 0)
            hp = 0;

        Debug.Log($"HP: {hp}");
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void ResetGame(int startHp = 3)
    {
        hp = startHp;
        score = 0;
    }
}