using UnityEngine;
using UnityEngine.InputSystem;

// This script handles movement and the new Wall Jump mechanic
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public CharacterController controller;
    public float maxSpeed = 15f;
    public float acceleration = 60f; 
    public float friction = 40f;     
    public float gravity = -30f; 
    public float jumpHeight = 3f;

    [Header("Wall Jump Settings")]
    public int maxWallBounces = 3;
    public float wallBounceForce = 18f; // Horizontal push away from wall
    public float wallJumpUpForce = 12f;  // Vertical boost
    private int _remainingWallBounces;
    private Vector3 _lastWallNormal;
    private float _wallContactTimer;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip jumpSound;

    [Header("Dash Shift Settings")]
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
    
    [Header("Leeway Settings")]
    public float coyoteTime = 0.15f;    
    private float _coyoteTimeCounter;   

    private Vector3 _horizontalVelocity; 
    private Vector3 _verticalVelocity;   
    private bool _isGrounded;

    public Vector3 HorizontalVelocity => _horizontalVelocity;
    public bool IsGrounded => _isGrounded;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded)
        {
            _coyoteTimeCounter = coyoteTime;
            // Reset wall bounces when touching the ground
            _remainingWallBounces = maxWallBounces;
            if (_verticalVelocity.y < 0) _verticalVelocity.y = -2f;
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
            _wallContactTimer -= Time.deltaTime;
        }

        if (_dashTimer > 0)
        {
            PerformDashShift();
            return;
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

        // --- JUMP & WALL BOUNCE LOGIC ---
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            if (_coyoteTimeCounter > 0f)
            {
                // Standard Jump
                ExecuteJump(jumpHeight);
                _coyoteTimeCounter = 0f;
            }
            else if (_remainingWallBounces > 0 && _wallContactTimer > 0)
            {
                // Wall Bounce
                PerformWallBounce();
            }
        }

        controller.Move((_horizontalVelocity + _verticalVelocity) * Time.deltaTime);
        _verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void ExecuteJump(float height)
    {
        // Physics formula for jump velocity: $v = \sqrt{h \cdot -2 \cdot g}$
        _verticalVelocity.y = Mathf.Sqrt(height * -2f * gravity);
        if (audioSource && jumpSound) audioSource.PlayOneShot(jumpSound);
    }

    private void PerformWallBounce()
    {
        _remainingWallBounces--;
        _wallContactTimer = 0; // Prevent double jumping off the same contact

        // Calculate bounce direction: away from the wall normal
        // $\vec{V}_{new} = \vec{N}_{wall} \cdot Force$
        _horizontalVelocity = _lastWallNormal * wallBounceForce;
        
        // Add vertical boost
        _verticalVelocity.y = wallJumpUpForce;

        if (audioSource && jumpSound) audioSource.PlayOneShot(jumpSound);
    }

    // This callback is triggered when the CharacterController hits a surface
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // We only care about wall hits while in the air
        if (!_isGrounded)
        {
            // Check if the surface is vertical enough to be a wall
            // A wall normal should have a small Y component
            if (Mathf.Abs(hit.normal.y) < 0.3f)
            {
                _lastWallNormal = hit.normal;
                _wallContactTimer = 0.2f; // Window of time to press jump after hitting
            }
        }
    }

    // --- DASH METHODS (from previous step) ---
    private void PerformDashShift()
    {
        controller.Move(_dashDirection * _dashPower * Time.deltaTime);
        _verticalVelocity.y = 0;
        _dashTimer -= Time.deltaTime;
        if (_dashTimer <= 0) _horizontalVelocity = _dashDirection * maxSpeed;
    }

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
}