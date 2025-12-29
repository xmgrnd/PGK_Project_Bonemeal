using UnityEngine;
using UnityEngine.SceneManagement;

// This script manages the high-level logic of the main menu, 
// including scene loading, UI panel toggling, and application exit.
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Management")]
    // The exact name of the scene to load when 'Play' is pressed.
    public string demoSceneName = "Level1";

    [Header("UI References")]
    // Reference to the Credits UI group. Needs to be assigned in the Inspector.
    public GameObject creditsPanel;

    // Loads the gameplay scene defined in demoSceneName.
    public void PlayDemo()
    {
        SceneManager.LoadScene(demoSceneName);
    }

    void Start()
    {
        // Ensure the credits panel is hidden when the game starts
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    // Activates the credits overlay panel.
    public void ShowCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);
        }
    }

    // Deactivates the credits overlay panel to return to the main menu.
    public void CloseCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);
        }
    }

    // Shuts down the application. Note: Only works in standalone builds.
    public void ExitGame()
    {
        Application.Quit();
    }
}