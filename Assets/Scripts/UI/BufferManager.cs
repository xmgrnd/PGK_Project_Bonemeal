using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

// Manages the transition between scenes with a typewriter effect and async loading
public class BufferManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI textDisplay;
    public string fullText = "Wchodzenie do strefy zagrożenia...";
    public float typingSpeed = 0.05f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip typeSound;

    [Header("Scene Loading")]
    public string nextSceneName = "Level1";
    public float delayAfterFinish = 1.5f;

    private AsyncOperation _asyncLoad;

    void Start()
    {
        // Start both processes at once
        StartCoroutine(TypeText());
        StartCoroutine(LoadSceneAsync());
    }

    // Handles the typewriter visual effect
    private IEnumerator TypeText()
    {
        textDisplay.text = "";
        
        foreach (char letter in fullText.ToCharArray())
        {
            textDisplay.text += letter;
            
            // Play sound for each letter
            if (audioSource && typeSound)
            {
                audioSource.PlayOneShot(typeSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        // Wait a bit after the text is fully displayed
        yield return new WaitForSeconds(delayAfterFinish);

        // Once text is done and scene is loaded, activate it
        if (_asyncLoad != null)
        {
            _asyncLoad.allowSceneActivation = true;
        }
    }

    // Loads the scene in the background without switching immediately
    private IEnumerator LoadSceneAsync()
    {
        _asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        
        // Prevent the scene from starting until the typewriter effect finishes
        _asyncLoad.allowSceneActivation = false;

        while (!_asyncLoad.isDone)
        {
            // Check progress: $progress \in [0, 0.9]$ while loading
            float progress = Mathf.Clamp01(_asyncLoad.progress / 0.9f);
            
            yield return null;
        }
    }
}