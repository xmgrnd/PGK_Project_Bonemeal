using UnityEngine;

// Projectile with adjusted targeting height and impact destruction logic.
public class EnemyProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 15f;
    public float damage = 15f;
    public float homingStrength = 2f; 
    public float lifeTime = 5f;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] animationFrames; 
    public float animationSpeed = 0.1f;

    private Transform _player;
    private Vector3 _direction;
    private int _currentFrame;
    private float _animTimer;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            // TARGETING FIX: Aim 0.5m above pivot (lower than before)
            Vector3 targetOffset = Vector3.up * 0.5f;
            _direction = (_player.position + targetOffset - transform.position).normalized;
        }
        
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (_player != null)
        {
            // Calculate target position with the same 0.5m offset
            Vector3 targetPos = _player.position + Vector3.up * 0.5f;
            Vector3 targetDir = (targetPos - transform.position).normalized;
            
            // Apply slight homing adjustment [cite: 2025-12-25]
            _direction = Vector3.Slerp(_direction, targetDir, homingStrength * Time.deltaTime);
        }

        transform.position += _direction * speed * Time.deltaTime;
        HandleAnimation();
    }

    private void HandleAnimation()
    {
        if (animationFrames == null || animationFrames.Length < 2) return;
        _animTimer += Time.deltaTime;
        if (_animTimer >= animationSpeed)
        {
            _currentFrame = (_currentFrame + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[_currentFrame];
            _animTimer = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hits Player
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // Ignore Enemies [cite: 2025-12-25]
        if (other.CompareTag("Enemy")) return;

        // NO BOUNCE: Destroy on any solid surface (Environment or Default layers)
        if (other.gameObject.layer == LayerMask.NameToLayer("Environment") || 
            other.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            Destroy(gameObject);
        }
    }
}