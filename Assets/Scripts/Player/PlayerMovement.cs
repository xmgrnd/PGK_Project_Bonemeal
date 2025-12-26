using UnityEngine;
using UnityEngine.InputSystem;

// This script handles player movement with a dedicated state for linear dashing
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float maxSpeed = 15f;
    public float acceleration = 60f; 
    public float friction = 40f;     
    public float gravity = -30f; 
    public float jumpHeight = 3f;

    [Header("Dash Shift Settings")]
    // How long the dash lasts (seconds)
    public float dashDuration = 0.2f; 
    private float _dashTimer;
    private Vector3 _dashDirection;
    private float _dashPower;

    [Header("Air Control Settings")]
    [Range(0, 1)]
    public float airControlMultiplier = 0.4f; 
    
    [Header("Ground Check Settings")]
    public Transform groundCheck;     
    public float groundDistance = 0.4f; 
    public LayerMask groundMask;      
    
    [Header("Leeway Settings (Coyote Time)")]
    public float coyoteTime = 0.15f;    
    private float _coyoteTimeCounter;   

    private Vector3 _horizontalVelocity; 
    private Vector3 _verticalVelocity;   
    private bool _isGrounded;

    // Public properties for other scripts (MouseLook, DashManager)
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public bool IsGrounded => _isGrounded;

    void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded)
        {
            _coyoteTimeCounter = coyoteTime;
            if (_verticalVelocity.y < 0) _verticalVelocity.y = -2f;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        // --- DASH STATE LOGIC ---
        if (_dashTimer > 0)
        {
            PerformDashShift();
            return; // Skip normal movement logic while dashing
        }

        HandleStandardMovement();
    }

    private void HandleStandardMovement()
    {
        var keyboard = Keyboard.current;
        Vector3 inputDirection = Vector3.zero;

        if (keyboard != null)
        {
            float x = 0; float z = 0;
            if (keyboard.wKey.isPressed) z += 1;
            if (keyboard.sKey.isPressed) z -= 1;
            if (keyboard.aKey.isPressed) x -= 1;
            if (keyboard.dKey.isPressed) x += 1;
            inputDirection = (transform.right * x + transform.forward * z).normalized;
        }

        Vector3 targetVelocity = inputDirection * maxSpeed;

        // Apply acceleration or friction based on ground state
        if (_isGrounded)
        {
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, 
                (inputDirection.magnitude > 0 ? acceleration : friction) * Time.deltaTime);
        }
        else
        {
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, 
                acceleration * airControlMultiplier * Time.deltaTime);
        }

        // Jumping
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && _coyoteTimeCounter > 0f)
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _coyoteTimeCounter = 0f;
        }

        // Normal move execution
        controller.Move((_horizontalVelocity + _verticalVelocity) * Time.deltaTime);
        _verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void PerformDashShift()
    {
        // Move the player at a constant speed in the dash direction
        controller.Move(_dashDirection * _dashPower * Time.deltaTime);

        // Reset vertical velocity so the player doesn't "plummet" after the dash ends
        _verticalVelocity.y = 0;

        _dashTimer -= Time.deltaTime;

        // When the dash ends, preserve some momentum
        if (_dashTimer <= 0)
        {
            _horizontalVelocity = _dashDirection * maxSpeed;
        }
    }

    // New Dash Initiation Method: Stores parameters and starts the timer
    public void StartDashShift(Vector3 direction, float power)
    {
        _dashDirection = direction;
        _dashPower = power;
        _dashTimer = dashDuration;
    }

    public Vector3 GetMovementInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return Vector3.zero;

        float x = 0; float z = 0;
        if (keyboard.wKey.isPressed) z += 1;
        if (keyboard.sKey.isPressed) z -= 1;
        if (keyboard.aKey.isPressed) x -= 1;
        if (keyboard.dKey.isPressed) x += 1;

        return (transform.right * x + transform.forward * z).normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}