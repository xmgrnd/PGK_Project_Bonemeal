using UnityEngine;
using UnityEngine.InputSystem;

// This script provides responsive FPS aiming with a cinematic camera tilt
// It combines influences from mouse movement, player strafing, dashing, and taking damage
public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    public float mouseSensitivity = 0.1f;
    
    [Header("Tilt Settings (Mouse)")]
    public float mouseTiltAmount = 1.5f; 
    public float mouseTiltSpeed = 10f; 

    [Header("Tilt Settings (Movement)")]
    public float moveTiltAmount = 2.0f; 
    public float moveTiltSpeed = 5f; 

    [Header("References")]
    public Transform playerBody;
    public PlayerMovement movementScript;
    public DashManager dashManager; 
    private PlayerHealth _playerHealth; // Added for damage feedback
    
    private float _xRotation = 0f;
    private float _currentMouseTilt = 0f;
    private float _currentMoveTilt = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        if (movementScript == null)
            movementScript = GetComponentInParent<PlayerMovement>();
            
        if (dashManager == null)
            dashManager = GetComponentInParent<DashManager>();

        // Find health component on the player body
        if (movementScript != null)
            _playerHealth = movementScript.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // --- 1. MOUSE LOOK LOGIC ---
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        playerBody.Rotate(Vector3.up * mouseX);

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        float targetMouseTilt = -mouseX * mouseTiltAmount;
        _currentMouseTilt = Mathf.Lerp(_currentMouseTilt, targetMouseTilt, Time.deltaTime * mouseTiltSpeed);

        // --- 2. MOVEMENT TILT LOGIC ---
        Vector3 localVelocity = playerBody.InverseTransformDirection(movementScript.HorizontalVelocity);
        float targetMoveTilt = -localVelocity.x * moveTiltAmount * 0.1f;
        _currentMoveTilt = Mathf.Lerp(_currentMoveTilt, targetMoveTilt, Time.deltaTime * moveTiltSpeed);

        // --- 3. EXTERNAL TILT INTEGRATION (Dash & Damage) ---
        float dashTilt = (dashManager != null) ? dashManager.GetCurrentDashTilt() : 0f;
        float damageTilt = (_playerHealth != null) ? _playerHealth.GetDamageTilt() : 0f;

        // --- 4. FINAL ROTATION ---
        // Combined Roll: $Roll_{total} = Tilt_{mouse} + Tilt_{movement} + Tilt_{dash} + Tilt_{damage}$
        float totalTilt = _currentMouseTilt + _currentMoveTilt + dashTilt + damageTilt;

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, totalTilt);

        if (Mathf.Abs(mouseX) < 0.01f)
        {
            _currentMouseTilt = Mathf.Lerp(_currentMouseTilt, 0f, Time.deltaTime * mouseTiltSpeed);
        }
    }
}