using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtonUI : MonoBehaviour
{
    [SerializeField] private string firstStageSceneName = "Stage1Scene";
    [SerializeField] private string titleSceneName = "TitleScene";

    public void StartGame()
    {
        // Delay resetting game state until the new scene has finished loading.
        // This avoids accessing scene-specific UI (e.g. Image) that will be destroyed
        // during the scene transition which can cause MissingReferenceException.
        SceneManager.sceneLoaded += OnSceneLoaded_StartGame;
        SceneManager.LoadScene(firstStageSceneName);
    }

    private void OnSceneLoaded_StartGame(Scene scene, LoadSceneMode mode)
    {
        // Only reset when the intended scene is loaded.
        if (scene.name == firstStageSceneName)
        {
            GameState.Instance.ResetGame(3);
        }

        // Unsubscribe so this runs only once.
        SceneManager.sceneLoaded -= OnSceneLoaded_StartGame;
    }

    public void GoToTitle()
    {
        SceneManager.LoadScene(titleSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}