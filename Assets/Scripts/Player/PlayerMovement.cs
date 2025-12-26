using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float speed = 15f;
    public float gravity = -30f; 
    public float jumpHeight = 3f;

    [Header("Air Control Settings")]
    [Range(0, 1)]
    public float airControlMultiplier = 0.2f; // Lower = less control in air
    private Vector3 _horizontalVelocity;      // Stores current XZ momentum

    [Header("Ground Check Settings")]
    public Transform groundCheck;     
    public float groundDistance = 0.4f; 
    public LayerMask groundMask;      
    
    [Header("Leeway Settings (Coyote Time)")]
    public float coyoteTime = 0.15f;    
    private float _coyoteTimeCounter;   

    private Vector3 _verticalVelocity; // Renamed from _velocity for clarity
    private bool _isGrounded;

    void Update()
    {
        // 1. Precise Ground Check
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

        // 2. Handle Input
        var keyboard = Keyboard.current;
        Vector3 targetMoveInput = Vector3.zero;

        if (keyboard != null)
        {
            float x = 0; float z = 0;
            if (keyboard.wKey.isPressed) z += 1;
            if (keyboard.sKey.isPressed) z -= 1;
            if (keyboard.aKey.isPressed) x -= 1;
            if (keyboard.dKey.isPressed) x += 1;

            targetMoveInput = (transform.right * x + transform.forward * z).normalized;
        }

        // 3. Apply Movement with Air Control Logic
        if (_isGrounded)
        {
            // On ground, movement is instant and responsive
            _horizontalVelocity = targetMoveInput * speed;
        }
        else
        {
            // In air, we use Lerp to slowly transition current momentum towards the desired input
            // This prevents the player from instantly reversing direction in mid-air
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, targetMoveInput * speed, airControlMultiplier * Time.deltaTime * 10f);
        }

        // 4. Handle Jumping
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && _coyoteTimeCounter > 0f)
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _coyoteTimeCounter = 0f;
        }

        // 5. Final Movement Execution
        // Combine smoothed horizontal momentum and vertical gravity
        Vector3 finalMove = _horizontalVelocity + _verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);

        // 6. Apply Gravity
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
}