using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Manages dash charges, UI, and visual/audio feedback with an inter-dash cooldown
public class DashManager : MonoBehaviour
{
    [Header("Dash Settings")]
    public int maxDashes = 3;
    public float dashRefillTime = 1.0f; // Time to refill one charge
    public float dashPower = 30f;
    public float dashCooldown = 0.5f;    // New: Minimum time between consecutive dashes

    [Header("Visual Feedback (Tilt)")]
    public Transform cameraTransform;
    public float dashTiltAngle = 5f;
    public float tiltReturnSpeed = 10f;
    private float _currentDashTilt;

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip dashSound;

    [Header("UI References")]
    public Image dashFillImage; // References dash_full.png with Fill Method

    [Header("References")]
    public PlayerMovement playerMovement;

    private float _currentDashes;
    private float _nextDashTime; // Internal timer to track when the next dash is allowed
    

    void Start()
    {
        _currentDashes = maxDashes;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // 1. Dash Charge Regeneration
        if (_currentDashes < maxDashes)
        {
            // Refill charges over time based on dashRefillTime
            _currentDashes += (1.0f / dashRefillTime) * Time.deltaTime;
            _currentDashes = Mathf.Min(_currentDashes, maxDashes);
        }

        // 2. Input Handling
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.leftShiftKey.wasPressedThisFrame)
        {
            AttemptDash();
        }

        // 3. Smoothly return the dash-induced tilt back to zero
        _currentDashTilt = Mathf.Lerp(_currentDashTilt, 0f, Time.deltaTime * tiltReturnSpeed);

        UpdateDashUI();
    }

    private void AttemptDash()
    {
        // Check if we have at least one charge AND enough time has passed since the last dash
        if (_currentDashes >= 1.0f && Time.time >= _nextDashTime)
        {
            Vector3 dashDirection = playerMovement.GetMovementInput();

            if (dashDirection == Vector3.zero)
                dashDirection = playerMovement.transform.forward;

            playerMovement.ApplyDashImpulse(dashDirection * dashPower);

            // Trigger Visual and Audio Feedback
            CalculateDashTilt(dashDirection);
            PlayDashSound();

            // Consume one charge and set the timestamp for the next available dash
            _currentDashes -= 1.0f;
            _nextDashTime = Time.time + dashCooldown;
        }
    }

    private void CalculateDashTilt(Vector3 worldDir)
    {
        // Convert world direction to local to determine left/right tilt
        Vector3 localDir = playerMovement.transform.InverseTransformDirection(worldDir);
        _currentDashTilt = -localDir.x * dashTiltAngle;
    }

    private void PlayDashSound()
    {
        if (audioSource != null && dashSound != null)
        {
            audioSource.PlayOneShot(dashSound);
        }
    }

    private void UpdateDashUI()
    {
        if (dashFillImage != null)
        {
            // UI reflects total charges using the filled sprite technique
            dashFillImage.fillAmount = _currentDashes / (float)maxDashes;
        }
    }

    // Property used by MouseLook to add dash tilt to the camera rotation
    public float GetCurrentDashTilt() => _currentDashTilt;
}