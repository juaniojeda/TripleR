using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MainMenuButtons : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private string creditsSceneName = "Credits";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenCredits()
    {
        SceneManager.LoadScene(creditsSceneName);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void RestartPlayerPref()
    {
        PlayerProfile.ResetProfile();
    }
}