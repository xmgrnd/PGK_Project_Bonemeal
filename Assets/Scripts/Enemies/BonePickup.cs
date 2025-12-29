using UnityEngine;

// This script handles the bone's physics: jumping out, waiting, 
// and then accelerating toward the player like a gravitational pull.
// Added: Auto-destruction logic to remove uncollected bones after a set time.
public class BonePickup : MonoBehaviour
{
    private enum BoneState { Jumping, Waiting, Flying }
    private BoneState _currentState = BoneState.Jumping;

    [Header("Settings")]
    public float healAmount = 5f;
    public float waitDuration = 2.0f;
    public float detectionRange = 10f;
    public float acceleration = 40f; 
    public float maxFlySpeed = 20f;
    public float jumpForce = 5f;

    [Header("Despawn Settings")]
    // Time in seconds before the bone disappears if not collected [cite: 2025-12-25]
    public float lifeTime = 5f; 

    [Header("Audio")]
    public AudioClip pickupSound;

    private Transform _player;
    private PlayerHealth _playerHealth; 
    private Vector3 _velocity;
    private float _waitTimer;
    private float _groundY;

    void Start()
    {
        // Finding the player and health system
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        // Initial jump velocity
        _velocity = new Vector3(Random.Range(-1f, 1f), 1.5f, Random.Range(-1f, 1f)).normalized * jumpForce;
        _groundY = transform.position.y;
        _waitTimer = waitDuration;

        // OPTIONAL: Simple way to destroy after X seconds if no complex logic is needed:
        // Destroy(gameObject, lifeTime); 
    }

    void Update()
    {
        // 1. AUTO-DESPAWN LOGIC: Count down the lifetime and destroy if it reaches zero [cite: 2025-12-25]
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
        {
            Destroy(gameObject);
            return; // Exit early as the object no longer exists
        }

        if (_player == null) return;

        // 2. STATE MACHINE LOGIC
        switch (_currentState)
        {
            case BoneState.Jumping:
                HandleJumping();
                break;
            case BoneState.Waiting:
                HandleWaiting();
                break;
            case BoneState.Flying:
                HandleFlying();
                break;
        }
    }

    private void HandleJumping()
    {
        transform.position += _velocity * Time.deltaTime;
        _velocity.y -= 9.81f * Time.deltaTime;

        if (transform.position.y <= _groundY)
        {
            transform.position = new Vector3(transform.position.x, _groundY, transform.position.z);
            _currentState = BoneState.Waiting;
        }
    }

    private void HandleWaiting()
    {
        _waitTimer -= Time.deltaTime;
        if (_waitTimer <= 0)
        {
            if (Vector3.Distance(transform.position, _player.position) < detectionRange)
            {
                _currentState = BoneState.Flying;
                _velocity = Vector3.zero; 
            }
        }
    }

    private void HandleFlying()
    {
        Vector3 direction = (_player.position + Vector3.up - transform.position).normalized;
        _velocity += direction * acceleration * Time.deltaTime;
        _velocity = Vector3.ClampMagnitude(_velocity, maxFlySpeed);
        transform.position += _velocity * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && _playerHealth != null)
        {
            if (_playerHealth.audioSource != null && pickupSound != null)
            {
                _playerHealth.audioSource.PlayOneShot(pickupSound);
            }

            _playerHealth.Heal(healAmount); 
            Destroy(gameObject);
        }
    }
}