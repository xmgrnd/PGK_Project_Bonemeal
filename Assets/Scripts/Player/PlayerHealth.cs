using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

// Comprehensive health system managing damage, healing, and the death sequence.
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float _currentHealth;
    private bool _isDead = false;

    [Header("Death Assets")]
    public GameObject mainHUDCanvas;     
    public GameObject crosshair;        
    public GameObject gameOverPanel;    
    public TextMeshProUGUI flashingText; 
    public Transform cameraTransform;   

    [Header("Visual Feedback")]
    public Volume postProcessVolume;
    public float damageVignetteIntensity = 0.5f;
    public Color damageColor = new Color(1f, 0f, 0f, 0.5f);
    public Color healColor = new Color(0f, 1f, 0f, 0.4f); // Green tint for healing 
    public float vignetteFadeSpeed = 4f;
    private Vignette _vignette;
    private float _currentVignetteIntensity;

    [Header("Camera Tilt")]
    public float damageTiltAmount = 5f;
    public float tiltFadeSpeed = 10f;
    private float _currentDamageTilt;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip deathSound;

    [Header("References")]
    public WeaponManager weaponManager;
    public PlayerMovement movementScript;
    public DashManager dashManager;
    public Image healthFillImage;
    public TextMeshProUGUI healthText;

    private float _animatedFillAmount;
    public bool isGodModeActive { get; set; } = false;
    
    void Start()
    {
        _currentHealth = maxHealth;
        _animatedFillAmount = 1f; 
        
        if (postProcessVolume != null && postProcessVolume.profile.TryGet(out _vignette))
        {
            _vignette.active = true;
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateHealthUI(true); 
    }

    void Update()
    {
        // Death state: Wait for any key to reload 
        if (_isDead)
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        // Smooth UI transition
        float targetFill = _currentHealth / maxHealth;
        _animatedFillAmount = Mathf.Lerp(_animatedFillAmount, targetFill, Time.deltaTime * 5f);
        if (healthFillImage != null) healthFillImage.fillAmount = _animatedFillAmount;
    
        // Visual recovery 
        _currentVignetteIntensity = Mathf.Lerp(_currentVignetteIntensity, 0f, Time.deltaTime * vignetteFadeSpeed);
        if (_vignette != null) _vignette.intensity.value = _currentVignetteIntensity;
        _currentDamageTilt = Mathf.Lerp(_currentDamageTilt, 0f, Time.deltaTime * tiltFadeSpeed);
    }

    // Restored: Logic for healing from BonePickups 
    public void Heal(float amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, maxHealth);

        // Flash green vignette on heal 
        if (_vignette != null)
        {
            _vignette.color.value = healColor;
            _currentVignetteIntensity = damageVignetteIntensity;
        }

        UpdateHealthUI(false);
    }

    public void TakeDamage(float amount)
    {
        // Least taxing check: immediately exit if god mode or player is already dead
        if (isGodModeActive || _isDead) return;

        _currentHealth = Mathf.Max(_currentHealth - amount, 0);
        ApplyDamageFeedback();
        UpdateHealthUI(false);

        if (_currentHealth <= 0) Die();
    }

    private void ApplyDamageFeedback()
    {
        if (audioSource != null && damageSound != null) audioSource.PlayOneShot(damageSound);
        if (_vignette != null)
        {
            _vignette.color.value = damageColor; 
            _currentVignetteIntensity = damageVignetteIntensity;
        }
        _currentDamageTilt = Random.Range(-1f, 1f) * damageTiltAmount;
    }

    public float GetDamageTilt() => _currentDamageTilt;

    private void Die()
    {
        _isDead = true;

        // Lock controls 
        if (movementScript != null) movementScript.enabled = false;
        if (dashManager != null) dashManager.enabled = false;
        if (cameraTransform.TryGetComponent<MouseLook>(out var look)) look.enabled = false;
        if (cameraTransform.TryGetComponent<HeadBob>(out var bob)) bob.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (weaponManager != null) weaponManager.ClearInventoryOnDeath();
        if (mainHUDCanvas != null) mainHUDCanvas.SetActive(false);
        if (crosshair != null) crosshair.SetActive(false);
    
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (flashingText != null) StartCoroutine(FlashTextRoutine());
        }

        if (audioSource && deathSound) audioSource.PlayOneShot(deathSound);
        StartCoroutine(DeathCameraAnimation());
    }

    private IEnumerator DeathCameraAnimation()
    {
        float duration = 1.0f;
        float elapsed = 0f;
        Vector3 startPos = cameraTransform.localPosition;
        Quaternion startRot = cameraTransform.localRotation;
        
        Vector3 targetPos = new Vector3(startPos.x, 0.2f, startPos.z);
        Quaternion targetRot = Quaternion.Euler(10f, startRot.eulerAngles.y, 80f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curve = t * t * (3f - 2f * t); // SmoothStep 

            cameraTransform.localPosition = Vector3.Lerp(startPos, targetPos, curve);
            cameraTransform.localRotation = Quaternion.Slerp(startRot, targetRot, curve);
            yield return null;
        }
    }

    private IEnumerator FlashTextRoutine()
    {
        while (_isDead)
        {
            flashingText.alpha = 1f;
            yield return new WaitForSeconds(0.6f);
            flashingText.alpha = 0f;
            yield return new WaitForSeconds(0.4f);
        }
    }

    private void UpdateHealthUI(bool instant)
    {
        if (instant) _animatedFillAmount = _currentHealth / maxHealth;
        if (healthText != null) healthText.text = Mathf.RoundToInt(_currentHealth).ToString();
    }
}