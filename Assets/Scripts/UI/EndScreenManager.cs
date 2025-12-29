using UnityEngine;
using UnityEngine.SceneManagement;

// Manages the transition from the End Screen back to the Main Menu
public class EndScreenManager : MonoBehaviour
{
    [Header("Navigation")]
    // Make sure this matches exactly the name of your Main Menu scene
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        // Ensure the cursor is visible and unlocked so the player can click the button
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Called by the UI Button's OnClick event
    public void LoadMainMenu()
    {
        Debug.Log("Returning to Main Menu...");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}