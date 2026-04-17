using TMPro;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUDUI : MonoBehaviour
{
    [Header("Heart UI")]
    [SerializeField] private Image[] heartImages;

    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Options")]
    [SerializeField] private string scoreFormat = "SCORE : {0}";
    [SerializeField] private string timeFormat = "TIME : {0}";
    [SerializeField] private string stageFormat = "STAGE : {0}";

    private void Update()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        RefreshHearts();
        RefreshScore();
        RefreshTime();
        RefreshStage();
    }

    private void RefreshHearts()
    {
        if (heartImages == null || heartImages.Length == 0)
            return;

        int currentHp = GameState.Instance.HP;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] == null)
                continue;

            heartImages[i].enabled = i < currentHp;
        }
    }

    private void RefreshScore()
    {
        if (scoreText == null)
            return;

        scoreText.text = string.Format(scoreFormat, GameState.Instance.Score);
    }

    private void RefreshTime()
    {
        if (timeText == null)
            return;

        int remain = 0;

        if (StageFlowManager.Instance != null)
            remain = StageFlowManager.Instance.RemainingSeconds;

        timeText.text = string.Format(timeFormat, remain);
    }

    private void RefreshStage()
    {
        if (stageText == null)
            return;

        string sceneName = SceneManager.GetActiveScene().name;

        // Scene 이름을 사람이 보기 좋은 스테이지 명으로 변환
        string displayName;

        // 예: "Stage1Scene" -> "Stage 1"
        Match m = Regex.Match(sceneName, @"Stage\s*(\d+)", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            displayName = $"Stage {m.Groups[1].Value}";
        }
        else
        {
            switch (sceneName)
            {
                case "ClearScene":
                    displayName = "Clear";
                    break;
                case "GameOverScene":
                    displayName = "Game Over";
                    break;
                default:
                    // Fallback: 그냥 씬 이름을 그대로 표시
                    displayName = sceneName;
                    break;
            }
        }

        stageText.text = string.Format(stageFormat, displayName);
    }
}