using UnityEngine;

// This script handles the classic vertical head bobbing motion while walking
public class HeadBob : MonoBehaviour
{
    [Header("Bob Settings")]
    // Frequency: How fast the head bobs
    public float walkingBobSpeed = 14f;
    // Amplitude: How far the head moves up and down
    public float bobAmount = 0.05f;
    // Smoothness: How fast the camera returns to the center
    public float smoothReturnSpeed = 10f;

    [Header("References")]
    public PlayerMovement movementScript;

    private float _timer = 0f;
    private Vector3 _defaultPosition;

    void Start()
    {
        // Store the initial local position of the camera to use as a baseline
        _defaultPosition = transform.localPosition;

        if (movementScript == null)
            movementScript = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        // Calculate the horizontal speed of the player
        float speed = movementScript.HorizontalVelocity.magnitude;

        // Condition: Only bob if the player is grounded and moving fast enough
        if (movementScript.IsGrounded && speed > 0.1f)
        {
            // Increase the timer based on the player's movement speed
            // This makes the bobbing faster when the player runs faster
            _timer += Time.deltaTime * walkingBobSpeed;

            // Apply the Sine wave formula: $y = \sin(t) \cdot A$
            // We use the local position to add the bobbing offset to the default Y
            float newY = _defaultPosition.y + Mathf.Sin(_timer) * bobAmount;
            transform.localPosition = new Vector3(_defaultPosition.x, newY, _defaultPosition.z);
        }
        else
        {
            // Reset the timer and smoothly lerp back to the default position when idle or in air
            _timer = 0;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _defaultPosition, Time.deltaTime * smoothReturnSpeed);
        }
    }
}