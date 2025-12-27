using UnityEngine;
using UnityEngine.InputSystem;

// This script provides responsive FPS aiming with cinematic camera tilt and weapon recoil
public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity Settings")]
    public float mouseSensitivity = 0.1f;
    
    [Header("Recoil Settings")]
    // How fast the camera returns to the original position after recoil
    public float recoilReturnSpeed = 10f;
    // Current vertical offset caused by weapon kick
    private float _currentRecoilX = 0f;

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
    private PlayerHealth _playerHealth;
    
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

        // --- 3. RECOIL RECOVERY ---
        // Smoothly return the recoil offset back to zero
        _currentRecoilX = Mathf.Lerp(_currentRecoilX, 0f, Time.deltaTime * recoilReturnSpeed);

        // --- 4. EXTERNAL TILT INTEGRATION ---
        float dashTilt = (dashManager != null) ? dashManager.GetCurrentDashTilt() : 0f;
        float damageTilt = (_playerHealth != null) ? _playerHealth.GetDamageTilt() : 0f;

        // --- 5. FINAL ROTATION ---
        // We subtract recoil from X to kick the camera upwards
        // $Rotation_{final} = (\theta_x - Recoil, 0, Roll_{total})$
        float totalTilt = _currentMouseTilt + _currentMoveTilt + dashTilt + damageTilt;
        transform.localRotation = Quaternion.Euler(_xRotation - _currentRecoilX, 0f, totalTilt);

        if (Mathf.Abs(mouseX) < 0.01f)
        {
            _currentMouseTilt = Mathf.Lerp(_currentMouseTilt, 0f, Time.deltaTime * mouseTiltSpeed);
        }
    }

    // Public method for weapons to call when firing
    public void AddRecoil(float amount)
    {
        _currentRecoilX += amount;
    }
}