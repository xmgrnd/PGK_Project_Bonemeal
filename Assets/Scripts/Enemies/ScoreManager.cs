using UnityEngine;
using TMPro;

// Manages the global score ("Krew") and updates the world-space billboard UI.
// Shows a specific message once the final goal (700 points) is reached.
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Settings")]
    public TextMeshPro billboardText;
    public int targetPoints = 350; // Initial target for progression
    
    private int _currentPoints = 0;
    
    public int CurrentPoints => _currentPoints;
    
    void Awake()
    {
        // Singleton pattern for easy access from other scripts 
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateBillboard();
    }

    public void AddPoints(int amount)
    {
        _currentPoints += amount;
        UpdateBillboard();
        
        // Logical check for the first target (350)
        if (_currentPoints >= targetPoints && _currentPoints < 700)
        {
            Debug.Log("<color=red>First goal reached!</color> Ranged enemies unlocked.");
        }
    }

    private void UpdateBillboard()
    {
        if (billboardText != null)
        {
            // FINAL THRESHOLD: Show portal message at 700 points 
            if (_currentPoints >= 700)
            {
                billboardText.text = "Portal odblokowany";
            }
            else
            {
                
                billboardText.text = $"Krew {_currentPoints}/{targetPoints}";
            }
        }
    }
}