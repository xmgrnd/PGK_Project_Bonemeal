using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Handles the toggle logic and UI feedback for the God Mode cheat.
public class GodModeController : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;
    public TextMeshProUGUI godModeText; // Reference to the UI text in the top-right 

    void Start()
    {
        // Ensure the text is hidden when the game starts
        if (godModeText != null) godModeText.gameObject.SetActive(false); 
    }

    void Update()
    {
        // Using Input System Package to detect the '6' key 
        if (Keyboard.current != null && Keyboard.current.digit6Key.wasPressedThisFrame)
        {
            ToggleGodMode();
        }
    }

    private void ToggleGodMode()
    {
        if (playerHealth == null) return;

        // Flip the current state
        bool newState = !playerHealth.isGodModeActive;
        playerHealth.isGodModeActive = newState; 

        // Update UI visibility based on the new state
        if (godModeText != null)
        {
            godModeText.gameObject.SetActive(newState);
        }

        Debug.Log($"<color=yellow>Cheat:</color> God Mode {(newState ? "Enabled" : "Disabled")}");
    }
}