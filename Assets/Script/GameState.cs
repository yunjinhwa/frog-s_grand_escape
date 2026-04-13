using UnityEngine;

public class GameState
{
    private static readonly GameState instance = new GameState();
    public static GameState Instance => instance;

    private int hp;
    private int score;

    public int HP => hp;
    public int Score => score;

    private GameState() { }
}
