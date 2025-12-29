using UnityEngine;
using UnityEngine.SceneManagement;

// Minimalistic script that only handles the scene transition when the player enters the trigger.
public class EndPortal : MonoBehaviour
{
    [Header("Scene Transition")]
    // The exact name of the scene to load (must be added to Build Settings)
    public string nextSceneName = "FarewellScene";

    private void OnTriggerEnter(Collider other)
    {
        // Detects the player using the 'Player' tag or the PlayerMovement component
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            Debug.Log("<color=gold>Victory:</color> Player entered the portal. Loading next scene.");
            
            // Load the specified scene via SceneManager
            SceneManager.LoadScene(nextSceneName);
        }
    }
}