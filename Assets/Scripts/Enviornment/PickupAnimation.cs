using UnityEngine;

// This script handles the visual "pickup" behavior: rotation and levitation.
// It is designed to be placed on a trigger object to make it stand out.
public class PickupAnimation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 100f; // Degrees per second
    public Vector3 rotationAxis = Vector3.up;

    [Header("Levitation Settings")]
    public float floatAmplitude = 0.25f; // How high it goes
    public float floatFrequency = 2f;    // How fast it bobs

    private Vector3 _startPos;

    void Start()
    {
        // Store the original position to oscillate around it [cite: 2025-12-25]
        _startPos = transform.position;
    }

    void Update()
    {
        // 1. Rotation around the specified axis
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);

        // 2. Levitation logic using a Sine wave
        // Formula: Position = StartPosition + Sin(Time * Frequency) * Amplitude
        float newY = _startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(_startPos.x, newY, _startPos.z);
    }
}