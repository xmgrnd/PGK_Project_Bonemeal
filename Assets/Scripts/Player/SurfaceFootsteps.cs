using UnityEngine;
using UnityEngine.InputSystem;

// This script plays footstep sounds based on the surface the player is walking on.
// It requires an AudioSource on the same GameObject.
[RequireComponent(typeof(AudioSource))]
public class SurfaceFootsteps : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] dirtClips;
    public AudioClip[] stoneClips;

    [Header("Step Settings")]
    public float stepInterval = 0.5f; // Time between steps
    public float rayDistance = 1.5f;  // Distance to check for ground
    public LayerMask groundLayer;     // Which layers to check

    private AudioSource _audioSource;
    private float _stepTimer;
    private bool _isMoving;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        CheckMovement();

        if (_isMoving)
        {
            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0)
            {
                PlayFootstep();
                _stepTimer = stepInterval;
            }
        }
        else
        {
            // Reset timer when standing still so the first step plays immediately [cite: 2025-12-25]
            _stepTimer = 0;
        }
    }

    private void CheckMovement()
    {
        // Using Input System: Check if WASD or Arrows are being pressed [cite: 2025-12-25]
        if (Keyboard.current != null)
        {
            Vector2 moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;

            _isMoving = moveInput.sqrMagnitude > 0;
        }
    }

    private void PlayFootstep()
    {
        RaycastHit hit;
        // Cast a ray downwards to detect the floor tag [cite: 2025-12-25]
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, groundLayer))
        {
            string surfaceTag = hit.collider.tag;
            AudioClip clipToPlay = null;

            if (surfaceTag == "Dirt")
            {
                clipToPlay = GetRandomClip(dirtClips);
            }
            else if (surfaceTag == "Stone")
            {
                clipToPlay = GetRandomClip(stoneClips);
            }

            if (clipToPlay != null)
            {
                // PlayOneShot allows overlapping sounds for more realism [cite: 2025-12-25]
                _audioSource.PlayOneShot(clipToPlay);
            }
        }
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}