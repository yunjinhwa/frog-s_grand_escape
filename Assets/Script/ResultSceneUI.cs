using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultSceneUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Formats")]
    [SerializeField] private string scoreFormat = "FINAL SCORE : {0}";
    [SerializeField] private string hpFormat = "REMAIN HP : {0}";

    private void Start()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        string currentScene = SceneManager.GetActiveScene().name;


        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, GameState.Instance.Score);
        }

        if (hpText != null)
        {
            hpText.text = string.Format(hpFormat, GameState.Instance.HP);
        }
    }
}