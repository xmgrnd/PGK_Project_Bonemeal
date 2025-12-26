using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Handles AI behavior including pathfinding, multi-state animations, and player interaction
public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Chasing, Attacking, Pain, Dying }
    public EnemyState currentState = EnemyState.Idle;

    [Header("Animation Sprites")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] idleFrames;
    public Sprite[] walkFrames;
    public Sprite[] attackFrames;
    public Sprite[] painFrames;
    public Sprite[] deathFrames;
    public float animationSpeed = 0.15f;

    [Header("Stats & Logic")]
    public float health = 50f;
    public float attackRange = 2.0f; 
    public float detectionRange = 20f;
    public float damage = 10f;
    public float painDuration = 0.3f;
    public float attackCooldown = 1.0f; 

    private NavMeshAgent _agent;
    private Transform _player;
    private PlayerHealth _playerHealth;
    private bool _isDead = false;
    private bool _canAttack = true; 
    private int _currentFrame = 0;
    private float _animTimer;
    
    // Reference to stop current animation when state changes
    private Coroutine _animationCoroutine;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        
        // Ensure the enemy is correctly placed on the NavMesh
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            _agent.Warp(hit.position);
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            _playerHealth = playerObj.GetComponent<PlayerHealth>();
        }
        
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (_isDead || currentState == EnemyState.Pain || _player == null || !_agent.isOnNavMesh) return;

        // Calculate Euclidean distance to player: $d = \sqrt{(P_x-E_x)^2 + (P_y-E_y)^2 + (P_z-E_z)^2}$
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleAnimation(idleFrames);
                if (distanceToPlayer < detectionRange) currentState = EnemyState.Chasing;
                break;

            case EnemyState.Chasing:
                _agent.isStopped = false; // Ensure agent is allowed to move
                _agent.SetDestination(_player.position); // Continuous path update
                HandleAnimation(walkFrames);
                
                if (distanceToPlayer <= attackRange && _canAttack) 
                {
                    ChangeState(EnemyState.Attacking);
                }
                break;

            case EnemyState.Attacking:
                // Logic handled by AttackSequence Coroutine
                break;
        }
    }

    // Helper method to clear current animation and start a new state
    private void ChangeState(EnemyState newState)
    {
        if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
        currentState = newState;

        if (newState == EnemyState.Attacking)
            _animationCoroutine = StartCoroutine(AttackSequence());
        else if (newState == EnemyState.Pain)
            _animationCoroutine = StartCoroutine(PainSequence());
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

    private IEnumerator AttackSequence()
    {
        _canAttack = false;
        _agent.isStopped = true; // Freeze movement to perform the attack

        // Play the attack animation
        foreach (Sprite frame in attackFrames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(animationSpeed);
        }

        // Apply damage if player is still in range after animation
        if (_player != null && Vector3.Distance(transform.position, _player.position) <= attackRange + 0.5f)
        {
            if (_playerHealth != null) _playerHealth.TakeDamage(damage);
        }

        _agent.isStopped = false; // Resume chasing
        currentState = EnemyState.Chasing;

        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        health -= amount;

        if (health <= 0) 
        {
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            StartCoroutine(DeathSequence());
        }
        else 
        {
            ChangeState(EnemyState.Pain);
        }
    }

    private IEnumerator PainSequence()
    {
        _agent.isStopped = true; // Stop movement during flinch
        
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
        currentState = EnemyState.Dying;

        foreach (Sprite frame in deathFrames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(animationSpeed);
        }

        if (TryGetComponent<Collider>(out Collider col)) col.enabled = false;
        Destroy(gameObject, 5f);
    }
}