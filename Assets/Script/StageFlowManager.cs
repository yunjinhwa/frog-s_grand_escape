using UnityEngine;
using UnityEngine.SceneManagement;

public class StageFlowManager : MonoBehaviour
{
    [Header("Optional UI")]
    [SerializeField] private bool resetGameStateOnStage1Start = true;

    private UniversalTimer timer;
    private float stageTimeLimit;
    private bool stageEnded;

    public static StageFlowManager Instance { get; private set; }

    public int RemainingSeconds
    {
        get
        {
            float remain = Mathf.Max(0f, stageTimeLimit - timer.CheckTimer());
            return Mathf.CeilToInt(remain);
        }
    }

    public float RemainingTimeFloat
    {
        get
        {
            return Mathf.Max(0f, stageTimeLimit - timer.CheckTimer());
        }
    }

    private void Awake()
    {
        Instance = this;
        timer = new UniversalTimer();
    }

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Stage1Scene" && resetGameStateOnStage1Start && !GameState.Instance.RunInitialized)
        {
            GameState.Instance.ResetGame(3);
        }

        stageTimeLimit = GetStageTimeLimit(currentScene);
        timer.ResetTimer();
        timer.StartTimer();
    }

    private void Update()
    {
        if (stageEnded)
            return;

        if (RemainingTimeFloat <= 0f)
        {
            HandleTimeout();
        }
    }

    public void CompleteStage()
    {
        if (stageEnded)
            return;

        stageEnded = true;
        timer.StopTimer();

        int remainSeconds = RemainingSeconds;
        string currentScene = SceneManager.GetActiveScene().name;
        string nextScene = GetNextScene(currentScene);

        // 스테이지 클리어 고정 보너스와 남은 시간 점수는 매 스테이지 지급
        GameState.Instance.AddScore(500);
        GameState.Instance.AddScore(remainSeconds * 20);

        // 마지막 스테이지를 클리어한 경우에만 남은 HP 보너스를 추가로 지급
        if (currentScene == "Stage3Scene")
        {
            GameState.Instance.AddScore(GameState.Instance.HP * 200);
            GameState.Instance.ClearRunState();
            SceneManager.LoadScene("ClearScene");
            return;
        }

        // 그 외의 경우는 다음 스테이지로 이동
        if (string.IsNullOrEmpty(nextScene))
        {
            // 안전장치: 다음 씬 정보가 없으면 클리어 씬으로 이동
            GameState.Instance.ClearRunState();
            SceneManager.LoadScene("ClearScene");
            return;
        }

        SceneManager.LoadScene(nextScene);
    }

    private void HandleTimeout()
    {
        if (stageEnded)
            return;

        stageEnded = true;
        timer.StopTimer();

        GameState.Instance.DecreaseHP(1);

        if (GameState.Instance.IsDead())
        {
            GameState.Instance.ClearRunState();
            SceneManager.LoadScene("GameOverScene");
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void HandlePlayerDeathAndRestartStage()
    {
        if (stageEnded)
            return;

        if (GameState.Instance.IsDead())
        {
            stageEnded = true;
            timer.StopTimer();
            GameState.Instance.ClearRunState();
            SceneManager.LoadScene("GameOverScene");
        }
    }

    private float GetStageTimeLimit(string sceneName)
    {
        switch (sceneName)
        {
            case "Stage1Scene": return 30f;
            case "Stage2Scene": return 25f;
            case "Stage3Scene": return 20f;
            default: return 0f;
        }
    }

    private string GetNextScene(string sceneName)
    {
        switch (sceneName)
        {
            case "Stage1Scene": return "Stage2Scene";
            case "Stage2Scene": return "Stage3Scene";
            case "Stage3Scene": return "ClearScene";
            default: return string.Empty;
        }
    }
}