using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Scenes")]
    public string gameSceneName = "Main"; // rename to your gameplay scene

    public void StartGame() {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()   => optionsPanel.SetActive(true);
    public void CloseOptions()  => optionsPanel.SetActive(false);

    public void OpenCredits()   => creditsPanel.SetActive(true);
    public void CloseCredits()  => creditsPanel.SetActive(false);

    public void QuitGame() {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
