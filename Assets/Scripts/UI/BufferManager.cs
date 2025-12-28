using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

// Manages scene loading with a virtual progress limit to prevent animation stuttering
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
    public float loadSpeedLimit = 0.3f; // 0.3 means 30% per second

    private AsyncOperation _asyncLoad;
    private float _virtualProgress = 0f;
    private bool _isLoaded = false;

    void Start()
    {
        // Set loading priority to low to keep animations smooth
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        if (promptText != null) promptText.gameObject.SetActive(false);

        StartCoroutine(LoadSceneAsync());
    }

    void Update()
    {
        if (_isLoaded)
        {
            HandleBlinking();

            // Input System Package interaction Dostosuj skrypty...]
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                ActivateScene();
            }
            else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                ActivateScene();
            }
        }
    }

    private IEnumerator LoadSceneAsync()
    {
        if (string.IsNullOrEmpty(nextSceneName)) yield break;

        _asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        _asyncLoad.allowSceneActivation = false;

        // Loop until both the real load and virtual progress are finished
        while (_virtualProgress < 1f)
        {
            // Calculate virtual progress based on the speed limit (e.g., 30% per second)
            // $\Delta p = \Delta t \cdot speed$
            _virtualProgress += Time.deltaTime * loadSpeedLimit;

            // Optional: Clamp virtual progress so it doesn't outpace the real loading progress (0.9 threshold)
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

    private void ActivateScene()
    {
        if (_asyncLoad != null) _asyncLoad.allowSceneActivation = true;
    }
}