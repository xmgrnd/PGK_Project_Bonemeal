using UnityEngine;
using UnityEngine.InputSystem;

// This script provides responsive FPS aiming with a cinematic camera tilt
// It combines influences from mouse movement, player strafing, and dashing
public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    public float mouseSensitivity = 0.1f;
    
    [Header("Tilt Settings (Mouse)")]
    public float mouseTiltAmount = 1.5f; 
    public float mouseTiltSpeed = 10f; 

    [Header("Tilt Settings (Movement)")]
    // Intensity of camera lean when moving sideways
    public float moveTiltAmount = 2.0f; 
    public float moveTiltSpeed = 5f; 

    [Header("References")]
    public Transform playerBody;
    public PlayerMovement movementScript;
    public DashManager dashManager; // Reference to access dash-specific tilt
    
    private float _xRotation = 0f;
    private float _currentMouseTilt = 0f;
    private float _currentMoveTilt = 0f;

    void Start()
    {
        // Standard FPS procedure: lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        
        // Auto-assign references if they are not set in the Inspector
        if (movementScript == null)
            movementScript = GetComponentInParent<PlayerMovement>();
            
        if (dashManager == null)
            dashManager = GetComponentInParent<DashManager>();
    }

    void Update()
    {
        // --- 1. MOUSE LOOK LOGIC ---
        // Get raw delta values from the New Input System
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // Yaw (Horizontal rotation) is applied to the entire player body
        playerBody.Rotate(Vector3.up * mouseX);

        // Pitch (Vertical rotation) is clamped to prevent the camera from flipping
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // Calculate procedural Roll based on mouse speed (Mouse Tilt)
        float targetMouseTilt = -mouseX * mouseTiltAmount;
        _currentMouseTilt = Mathf.Lerp(_currentMouseTilt, targetMouseTilt, Time.deltaTime * mouseTiltSpeed);

        // --- 2. MOVEMENT TILT LOGIC ---
        // We calculate local velocity to see if the player is strafing left or right
        Vector3 localVelocity = playerBody.InverseTransformDirection(movementScript.HorizontalVelocity);
        
        // Target tilt is based on the X component of our local movement
        float targetMoveTilt = -localVelocity.x * moveTiltAmount * 0.1f; 
        _currentMoveTilt = Mathf.Lerp(_currentMoveTilt, targetMoveTilt, Time.deltaTime * moveTiltSpeed);

        // --- 3. DASH TILT INTEGRATION ---
        float dashTilt = 0f;
        if (dashManager != null)
        {
            // Fetch the current tilt value calculated in DashManager
            dashTilt = dashManager.GetCurrentDashTilt();
        }

        // --- 4. FINAL ROTATION ---
        // Sum all tilt influences (Mouse + Movement + Dash) for the final Roll (Z axis)
        float totalTilt = _currentMouseTilt + _currentMoveTilt + dashTilt;

        // Apply final rotation: Pitch (X), Yaw (0, handled by body), and combined Roll (Z)
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, totalTilt);

        // Smoothly reset mouse tilt to zero when the mouse stops moving
        if (Mathf.Abs(mouseX) < 0.01f)
        {
            _currentMouseTilt = Mathf.Lerp(_currentMouseTilt, 0f, Time.deltaTime * mouseTiltSpeed);
        }
    }
}