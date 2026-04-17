using UnityEngine;
using System;

public class GameState
{
    private static readonly GameState instance = new GameState();
    public static GameState Instance => instance;

    private int hp;
    private int score;
    private bool runInitialized;

    public int HP => hp;
    public int Score => score;
    public bool RunInitialized => runInitialized;

    private GameState()
    {
        hp = 3;
        score = 0;
        runInitialized = false;
    }

    public void ResetGame(int startHp = 3)
    {
        hp = startHp;
        score = 0;
        runInitialized = true;
    }

    public void MarkRunStarted()
    {
        runInitialized = true;
    }

    public void ClearRunState()
    {
        runInitialized = false;
    }

    public void DecreaseHP(int amount = 1)
    {
        hp -= amount;
        if (hp < 0)
            hp = 0;

        Debug.Log($"HP: {hp}");
    }

    public bool IsDead()
    {
        return hp <= 0;
    }

    public void AddScore(int amount)
    {
        int oldScore = score;
        score += amount;
        if (score < 0)
            score = 0;

        string sign = amount >= 0 ? $"+{amount}" : amount.ToString();

        // 가능한 한 상세한 호출 정보 수집 (메서드/파일/라인 확보가 불가능할 수 있어 안전한 방법 사용)
        string callerInfo = "Unknown";
        try
        {
            // Unity의 StackTraceUtility를 사용해서 안전하게 스택 정보를 추출
            string full = UnityEngine.StackTraceUtility.ExtractStackTrace();
            if (!string.IsNullOrEmpty(full))
            {
                // 첫 번째 유의미한 라인을 사용
                var lines = full.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 0)
                    callerInfo = lines[0].Trim();
            }
        }
        catch (Exception ex)
        {
            callerInfo = $"StackTrace unavailable: {ex.Message}";
        }

        string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Debug.Log($"[{time}] Score changed: {sign} -> New Score: {score} (was {oldScore}) by {callerInfo}");
    }

    public void AddForwardStepScore()
    {
        AddScore(10);
    }

    public void AddStageClearScore(int remainingSeconds)
    {
        AddScore(500);
        AddScore(remainingSeconds * 20);
        AddScore(hp * 200);
    }
}