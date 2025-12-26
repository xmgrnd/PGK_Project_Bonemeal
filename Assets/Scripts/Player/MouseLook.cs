using UnityEngine;
using UnityEngine.InputSystem;

// This script provides responsive FPS aiming with a cinematic camera tilt reacting to mouse and movement
public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    public float mouseSensitivity = 0.1f;
    
    [Header("Tilt Settings (Mouse)")]
    public float mouseTiltAmount = 1.5f; 
    public float mouseTiltSpeed = 10f; 

    [Header("Tilt Settings (Movement)")]
    // How much the camera tilts when strafing left/right
    public float moveTiltAmount = 2.0f; 
    // How fast the camera reaches the movement tilt angle
    public float moveTiltSpeed = 5f; 

    [Header("References")]
    public Transform playerBody;
    public PlayerMovement movementScript;
    
    private float _xRotation = 0f;
    private float _currentMouseTilt = 0f;
    private float _currentMoveTilt = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        
        // Auto-assign the movement script if it's on the parent object
        if (movementScript == null)
        {
            movementScript = GetComponentInParent<PlayerMovement>();
        }
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

        // Calculate Mouse Tilt (Roll)
        float targetMouseTilt = -mouseX * mouseTiltAmount;
        _currentMouseTilt = Mathf.Lerp(_currentMouseTilt, targetMouseTilt, Time.deltaTime * mouseTiltSpeed);

        // --- 2. MOVEMENT TILT LOGIC ---
        // We find the "Local" side-to-side velocity using InverseTransformDirection
        Vector3 localVelocity = playerBody.InverseTransformDirection(movementScript.HorizontalVelocity);
        
        // If localVelocity.x is positive, we are strafing right -> tilt left (negative)
        float targetMoveTilt = -localVelocity.x * moveTiltAmount * 0.1f; 
        _currentMoveTilt = Mathf.Lerp(_currentMoveTilt, targetMoveTilt, Time.deltaTime * moveTiltSpeed);

        // --- 3. FINAL ROTATION ---
        // The total roll is the sum of both mouse and movement influences
        float totalTilt = _currentMouseTilt + _currentMoveTilt;

        // Apply instant Pitch and Yaw, while Roll stays smoothed
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, totalTilt);

        // Gently return mouse tilt to zero if mouse is not moving
        if (Mathf.Abs(mouseX) < 0.01f)
        {
            _currentMouseTilt = Mathf.Lerp(_currentMouseTilt, 0f, Time.deltaTime * mouseTiltSpeed);
        }
    }
}