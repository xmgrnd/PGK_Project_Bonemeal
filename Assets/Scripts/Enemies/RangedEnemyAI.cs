using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Improved AI script that uses a timestamp-based cooldown to prevent 
// attack freezing when taking damage during the attack sequence.
public class RangedEnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking, Pain, Dying }
    public EnemyState currentState = EnemyState.Idle;

    [Header("Stats")]
    public float health = 300f;
    public float moveSpeed = 2.5f;
    public float detectionRange = 25f;
    public float attackRange = 15f;
    public float attackCooldown = 2.0f;
    public float painDuration = 0.4f;

    [Header("Projectile and rewards Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public GameObject bonePrefab;

    [Header("Animation & Visuals")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleFrames, walkFrames, attackFrames, painFrames, deathFrames;
    public float animationSpeed = 0.15f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip spawnSound, shootSound, painSound, deathSound;

    private NavMeshAgent _agent;
    private Transform _player;
    private bool _isDead = false;
    
    // REPLACEMENT: Using a timestamp instead of a boolean flag [cite: 2025-12-25]
    private float _nextAttackTime = 0f; 
    
    private int _currentFrame = 0;
    private float _animTimer;
    private Coroutine _activeCoroutine;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = moveSpeed;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource && spawnSound) audioSource.PlayOneShot(spawnSound);
    }

    void Update()
    {
        if (_isDead || currentState == EnemyState.Pain || _player == null || !_agent.isOnNavMesh) return;

        float distance = Vector3.Distance(transform.position, _player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleAnimation(idleFrames);
                if (distance < detectionRange) currentState = EnemyState.Chasing;
                break;

            case EnemyState.Chasing:
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
                if (_agent.velocity.magnitude > 0.1f) HandleAnimation(walkFrames);
                else HandleAnimation(idleFrames);
                
                // CHECK: Only attack if enough time has passed since the last shot [cite: 2025-12-25]
                if (distance <= attackRange && Time.time >= _nextAttackTime) 
                {
                    ChangeState(EnemyState.Attacking);
                }
                break;
        }
    }

    private void ChangeState(EnemyState newState)
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        
        _currentFrame = 0;
        _animTimer = 0;
        currentState = newState;

        if (newState == EnemyState.Attacking)
            _activeCoroutine = StartCoroutine(RangedAttackSequence());
        else if (newState == EnemyState.Pain)
            _activeCoroutine = StartCoroutine(PainSequence());
        else if (newState == EnemyState.Dying)
            _activeCoroutine = StartCoroutine(DeathSequence());
    }

    private void HandleAnimation(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return;
        _animTimer += Time.deltaTime;
        if (_animTimer >= animationSpeed)
        {
            _currentFrame = (_currentFrame + 1) % frames.Length;
            spriteRenderer.sprite = frames[_currentFrame];
            _animTimer = 0;
        }
    }

    private IEnumerator RangedAttackSequence()
    {
        _agent.isStopped = true;

        // Play the attack animation frames
        foreach (Sprite frame in attackFrames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(animationSpeed);
        }

        // Shooting logic
        if (projectilePrefab != null && firePoint != null && !_isDead)
        {
            if (shootSound) audioSource.PlayOneShot(shootSound);
            Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        }

        // SET COOLDOWN: Define when the next shot can happen [cite: 2025-12-25]
        _nextAttackTime = Time.time + attackCooldown;

        _agent.isStopped = false;
        currentState = EnemyState.Chasing;
        
        // No more waiting inside the coroutine! This prevents the "stuck" bug.
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        health -= amount;
        if (health <= 0) ChangeState(EnemyState.Dying);
        else 
        {
            if (painSound) audioSource.PlayOneShot(painSound);
            ChangeState(EnemyState.Pain);
        }
    }

    private IEnumerator PainSequence()
    {
        _agent.isStopped = true;
        if (painFrames.Length > 0)
            spriteRenderer.sprite = painFrames[Random.Range(0, painFrames.Length)];
        yield return new WaitForSeconds(painDuration);
        _agent.isStopped = false;
        currentState = EnemyState.Chasing;
    }

    private IEnumerator DeathSequence()
    {
        _isDead = true;
        _agent.enabled = false;

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(50);
        if (deathSound) audioSource.PlayOneShot(deathSound);

        if (bonePrefab != null)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 spawnPos = transform.position + Vector3.up + Random.insideUnitSphere * 0.5f;
                Instantiate(bonePrefab, spawnPos, Quaternion.identity);
            }
        }

        foreach (Sprite frame in deathFrames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(animationSpeed);
        }

        if (TryGetComponent<Collider>(out Collider col)) col.enabled = false;
        Destroy(gameObject, 3f);
    }
}