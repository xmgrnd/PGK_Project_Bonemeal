using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Manages health logic with smooth UI animations and integrated damage feedback
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float _currentHealth;

    [Header("Smoothing Settings")]
    public float lerpSpeed = 5f; 
    private float _animatedFillAmount; 

    [Header("Damage Feedback - Visual")]
    public Volume postProcessVolume;
    public float damageVignetteIntensity = 0.5f;
    public Color damageColor = new Color(1f, 0f, 0f, 0.5f);
    public float vignetteFadeSpeed = 4f;
    private Vignette _vignette;
    private float _currentVignetteIntensity;

    [Header("Damage Feedback - Head Tilt")]
    // Intensity of the random camera roll when hit
    public float damageTiltAmount = 5f;
    public float tiltFadeSpeed = 10f;
    private float _currentDamageTilt;

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip damageSound;

    [Header("UI References")]
    public Image healthFillImage;
    public TextMeshProUGUI healthText; 

    void Start()
    {
        _currentHealth = maxHealth;
        _animatedFillAmount = 1f; 
        
        // Setup Post-processing reference
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out _vignette))
        {
            _vignette.active = true;
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        UpdateHealthUI(true); 
    }

    void Update()
    {
        // 1. Smooth Health Bar Animation
        float targetFill = _currentHealth / maxHealth;
        _animatedFillAmount = Mathf.Lerp(_animatedFillAmount, targetFill, Time.deltaTime * lerpSpeed);

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = _animatedFillAmount;
        }

        // 2. Smooth Feedback Recovery (Vignette & Tilt)
        _currentVignetteIntensity = Mathf.Lerp(_currentVignetteIntensity, 0f, Time.deltaTime * vignetteFadeSpeed);
        if (_vignette != null)
        {
            _vignette.intensity.value = _currentVignetteIntensity;
            _vignette.color.value = damageColor;
        }

        _currentDamageTilt = Mathf.Lerp(_currentDamageTilt, 0f, Time.deltaTime * tiltFadeSpeed);
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        // Trigger damage feedback effects
        ApplyDamageFeedback();

        UpdateHealthUI(false);

        if (_currentHealth <= 0) Die();
    }

    private void ApplyDamageFeedback()
    {
        // Play hit sound
        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Trigger red screen flash
        _currentVignetteIntensity = damageVignetteIntensity;

        // Apply a random head tilt direction: $Tilt = \text{Random}(-1, 1) \cdot Amount$
        _currentDamageTilt = Random.Range(-1f, 1f) * damageTiltAmount;
    }

    private void UpdateHealthUI(bool instant)
    {
        if (instant) _animatedFillAmount = _currentHealth / maxHealth;

        if (healthText != null)
        {
            healthText.text = Mathf.RoundToInt(_currentHealth).ToString();
        }
    }

    // Public getter for MouseLook to apply the hit-shake
    public float GetDamageTilt() => _currentDamageTilt;

    public void Heal(float amount)
    {
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);
        UpdateHealthUI(false);
    }

    private void Die()
    {
        Debug.Log("Player has died.");
    }
}