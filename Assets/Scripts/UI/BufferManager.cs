using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

// Manages scene loading with a virtual progress limit and single-input protection
public class BufferManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI promptText;
    public string nextSceneName = "Level1";

    [Header("Blinking Settings")]
    public float blinkSpeed = 2f;
    public float minAlpha = 0.1f;
    public float maxAlpha = 1.0f;

    [Header("Loading Limits")]
    [Range(0.1f, 1.0f)]
    public float loadSpeedLimit = 0.3f; 

    private AsyncOperation _asyncLoad;
    private float _virtualProgress = 0f;
    private bool _isLoaded = false;
    
    // Protection flag to ensure input is processed only once
    private bool _inputReceived = false;

    void Start()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        if (promptText != null) promptText.gameObject.SetActive(false);

        StartCoroutine(LoadSceneAsync());
    }

    void Update()
    {
        // Only allow input if the scene is ready AND we haven't received input yet
        if (_isLoaded && !_inputReceived)
        {
            HandleBlinking();

            // Detect first input and lock the state
            if (CheckAnyInput())
            {
                _inputReceived = true;
                
                // Optional: Stop blinking and set to full alpha to show it's loading
                StopBlinkingOnSelection();
                
                ActivateScene();
            }
        }
    }

    private bool CheckAnyInput()
    {
        // Check for any key or mouse click via Input System Package
        bool keyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        
        return keyPressed || mousePressed;
    }

    private IEnumerator LoadSceneAsync()
    {
        if (string.IsNullOrEmpty(nextSceneName)) yield break;

        _asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        _asyncLoad.allowSceneActivation = false;

        while (_virtualProgress < 1f)
        {
            // Calculation: $\Delta p = \Delta t \cdot speed$
            _virtualProgress += Time.deltaTime * loadSpeedLimit;

            float realProgress = Mathf.Clamp01(_asyncLoad.progress / 0.9f);
            float currentLimit = Mathf.Min(_virtualProgress, realProgress);

            if (currentLimit >= 1f && _asyncLoad.progress >= 0.9f)
            {
                _isLoaded = true;
                break;
            }

            yield return null;
        }

        if (promptText != null) promptText.gameObject.SetActive(true);
    }

    private void HandleBlinking()
    {
        if (promptText == null) return;
        
        float lerpAlpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
        Color newColor = promptText.color;
        newColor.a = Mathf.Lerp(minAlpha, maxAlpha, lerpAlpha);
        promptText.color = newColor;
    }

    private void StopBlinkingOnSelection()
    {
        if (promptText == null) return;
        Color c = promptText.color;
        c.a = maxAlpha;
        promptText.color = c;
    }

    private void ActivateScene()
    {
        if (_asyncLoad != null)
        {
            _asyncLoad.allowSceneActivation = true;
        }
    }
}