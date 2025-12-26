using UnityEngine;
using UnityEngine.InputSystem;

// This script provides responsive FPS aiming with a cinematic camera tilt
public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    public float mouseSensitivity = 0.1f;
    
    [Header("Tilt Settings")]
    // How much the camera rolls when turning
    public float tiltAmount = 1.5f; 
    // How fast the camera reaches the tilt angle
    public float tiltSpeed = 10f; 
    // How fast the camera returns to horizontal
    public float tiltReturnSpeed = 5f; 

    public Transform playerBody;
    
    private float _xRotation = 0f;
    private float _currentTilt = 0f;

    void Start()
    {
        // Ensure the cursor is locked for a focused FPS experience
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get mouse movement from the New Input System
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        // 1. Instant Rotation (Yaw & Pitch)
        // We apply Yaw directly to the body for 1:1 movement
        playerBody.Rotate(Vector3.up * mouseX);

        // Calculate Pitch and clamp it to prevent flipping over
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // 2. Smooth Tilt (Roll) Calculation
        if (Mathf.Abs(mouseX) > 0.1f)
        {
            // If moving the mouse, lean towards the target tilt
            float targetTilt = -mouseX * tiltAmount;
            _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        }
        else
        {
            // If the mouse is still, smoothly return to 0 (flat horizon)
            _currentTilt = Mathf.Lerp(_currentTilt, 0f, Time.deltaTime * tiltReturnSpeed);
        }

        // 3. Final Application
        // X = Pitch (Instant), Y = 0 (Body handles Yaw), Z = Roll (Smoothed)
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, _currentTilt);
    }
}