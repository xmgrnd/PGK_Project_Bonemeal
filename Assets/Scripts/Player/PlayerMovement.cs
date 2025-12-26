using UnityEngine;
using UnityEngine.InputSystem;

// This script handles player movement with physics-inspired acceleration and friction
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float maxSpeed = 15f;
    public float acceleration = 60f; 
    public float friction = 40f;     
    public float gravity = -30f; 
    public float jumpHeight = 3f;

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
    

    // Public property to let other scripts read the current horizontal momentum
    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public bool IsGrounded => _isGrounded;

    void Update()
    {
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

        if (_isGrounded)
        {
            if (inputDirection.magnitude > 0)
            {
                _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
            }
            else
            {
                _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, Vector3.zero, friction * Time.deltaTime);
            }
        }
        else
        {
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, targetVelocity, acceleration * airControlMultiplier * Time.deltaTime);
        }

        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && _coyoteTimeCounter > 0f)
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _coyoteTimeCounter = 0f;
        }

        Vector3 finalMove = _horizontalVelocity + _verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        _verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
    
   
    // Returns the normalized direction based on WASD input for dash direction calculation
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

    // Applies an external impulse (like a dash) directly to the horizontal momentum
    public void ApplyDashImpulse(Vector3 impulse)
    {
        // We override current horizontal velocity with the high-speed dash impulse
        _horizontalVelocity = impulse;
    }
}