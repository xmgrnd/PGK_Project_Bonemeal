using UnityEngine;
using UnityEngine.InputSystem;

// This script handles player movement with physics-inspired acceleration and friction
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float maxSpeed = 15f;
    public float acceleration = 60f; // How fast the player reaches maxSpeed
    public float friction = 40f;     // How fast the player stops when no input is given
    public float gravity = -30f; 
    public float jumpHeight = 3f;

    [Header("Air Control Settings")]
    [Range(0, 1)]
    public float airControlMultiplier = 0.4f; // Lower = harder to change direction in air
    
    [Header("Ground Check Settings")]
    public Transform groundCheck;     
    public float groundDistance = 0.4f; 
    public LayerMask groundMask;      
    
    [Header("Leeway Settings (Coyote Time)")]
    public float coyoteTime = 0.15f;    
    private float _coyoteTimeCounter;   

    private Vector3 _horizontalVelocity; // Persistent momentum on the XZ plane
    private Vector3 _verticalVelocity;   // Y-axis velocity (Gravity/Jumping)
    private bool _isGrounded;

    void Update()
    {
        // 1. Ground Check Logic
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded)
        {
            _coyoteTimeCounter = coyoteTime;
            if (_verticalVelocity.y < 0)
            {
                _verticalVelocity.y = -2f;
            }
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        // 2. Process Input
        var keyboard = Keyboard.current;
        Vector3 inputDirection = Vector3.zero;

        if (keyboard != null)
        {
            float x = 0; float z = 0;
            if (keyboard.wKey.isPressed) z += 1;
            if (keyboard.sKey.isPressed) z -= 1;
            if (keyboard.aKey.isPressed) x -= 1;
            if (keyboard.dKey.isPressed) x += 1;

            // Calculate world-space direction based on player rotation
            inputDirection = (transform.right * x + transform.forward * z).normalized;
        }

        // 3. Momentum and Friction Logic
        // Calculate target velocity based on input
        Vector3 targetVelocity = inputDirection * maxSpeed;

        if (_isGrounded)
        {
            if (inputDirection.magnitude > 0)
            {
                // Accelerate towards target velocity
                _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
            }
            else
            {
                // No input: apply friction to slow down naturally
                _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, Vector3.zero, friction * Time.deltaTime);
            }
        }
        else
        {
            // Air Control: Use a fraction of acceleration to steer in mid-air
            // This maintains existing momentum while allowing slight adjustments
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, acceleration * airControlMultiplier * Time.deltaTime);
        }

        // 4. Handle Jumping
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && _coyoteTimeCounter > 0f)
        {
            // Formula: $v = \sqrt{height \cdot -2 \cdot gravity}$
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _coyoteTimeCounter = 0f;
        }

        // 5. Apply Final Movement
        // Combine horizontal momentum and vertical gravity into one move call
        Vector3 finalMove = _horizontalVelocity + _verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        // 6. Apply Gravity
        _verticalVelocity.y += gravity * Time.deltaTime;
    }

    // Visualizes the ground check sphere in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}