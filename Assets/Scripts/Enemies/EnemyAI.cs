using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Comprehensive AI with pathfinding, combat animations, and specialized audio feedback
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

    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip[] painSounds;      // Randomly picks one of 2 sounds when hit
    public AudioClip deathSound;       // Plays once on death
    public AudioClip[] ambientSounds;   // Randomly moans while walking/idling
    public float minAmbientDelay = 5f;
    public float maxAmbientDelay = 12f;
    private float _nextAmbientTime;

    [Header("Stats & Logic")]
    public float health = 50f;
    public float attackRange = 2.0f; 
    public float detectionRange = 20f;
    public float damage = 10f;
    public float painDuration = 0.3f;
    public float attackCooldown = 1.0f; 
    public GameObject bonePrefab;

    private NavMeshAgent _agent;
    private Transform _player;
    private PlayerHealth _playerHealth;
    private bool _isDead = false;
    private bool _canAttack = true; 
    private int _currentFrame = 0;
    private float _animTimer;
    private Coroutine _animationCoroutine;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        
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
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Set initial random delay for first ambient sound
        _nextAmbientTime = Time.time + Random.Range(minAmbientDelay, maxAmbientDelay);
    }

    void Update()
    {
        if (_isDead || currentState == EnemyState.Pain || _player == null || !_agent.isOnNavMesh) return;

        HandleAmbientSounds();

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleAnimation(idleFrames);
                if (distanceToPlayer < detectionRange) currentState = EnemyState.Chasing;
                break;

            case EnemyState.Chasing:
                _agent.isStopped = false;
                _agent.SetDestination(_player.position);
                HandleAnimation(walkFrames);
                
                if (distanceToPlayer <= attackRange && _canAttack) 
                {
                    ChangeState(EnemyState.Attacking);
                }
                break;
        }
    }

    private void HandleAmbientSounds()
    {
        // Periodically play moans or growls if not dead or in pain
        if (Time.time >= _nextAmbientTime && ambientSounds.Length > 0)
        {
            AudioClip clip = ambientSounds[Random.Range(0, ambientSounds.Length)];
            audioSource.PlayOneShot(clip);
            _nextAmbientTime = Time.time + Random.Range(minAmbientDelay, maxAmbientDelay);
        }
    }

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
        _agent.isStopped = true;

        foreach (Sprite frame in attackFrames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(animationSpeed);
        }

        if (_player != null && Vector3.Distance(transform.position, _player.position) <= attackRange + 0.5f)
        {
            if (_playerHealth != null) _playerHealth.TakeDamage(damage);
        }

        _agent.isStopped = false;
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
            // Play random pain sound
            if (painSounds.Length > 0)
                audioSource.PlayOneShot(painSounds[Random.Range(0, painSounds.Length)]);

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
        // 1. Initial State Setup
        _isDead = true;
        _agent.enabled = false;
        currentState = EnemyState.Dying; //

        // 2. Score Management: Adds 25 points to the global "Krew" counter
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(25);
        }

        // 3. Audio Feedback: Plays the death scream once
        if (deathSound != null) 
        {
            audioSource.PlayOneShot(deathSound); //
        }

        // 4. Bone Spawning: Instantiates 2 bones that pop out and wait for pickup
        if (bonePrefab != null)
        {
            for (int i = 0; i < 2; i++)
            {
                // Spawn bones at the enemy's feet with a slight vertical offset
                Instantiate(bonePrefab, transform.position + Vector3.up, Quaternion.identity);
            }
        }

        // 5. Animation Loop: Cycles through the death frames sprite by sprite
        foreach (Sprite frame in deathFrames)
        {
            if (frame != null)
            {
                spriteRenderer.sprite = frame;
                yield return new WaitForSeconds(animationSpeed);
            }
        }

        // 6. Cleanup: Disable collider so player doesn't bump into the invisible corpse
        if (TryGetComponent<Collider>(out Collider col)) 
        {
            col.enabled = false;
        }
    
        // Remove the object from the scene after 5 seconds to save resources
        Destroy(gameObject, 5f);
    }
}