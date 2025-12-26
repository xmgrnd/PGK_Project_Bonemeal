using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

// Handles shotgun logic: multi-pellet raycast, spread, and heavy procedural bobbing
public class Shotgun : MonoBehaviour
{
    [Header("Visuals (Spritesheet)")]
    public Image weaponDisplay;       
    public Sprite idleSprite;
    public Sprite[] fireAnimation;    
    public float frameRate = 0.08f; // Shotguns usually have snappier animations

    [Header("Weapon Bobbing Settings")]
    public float bobSpeed = 10f;
    public float bobAmountX = 25f; // Wider sway for heavier weapon
    public float bobAmountY = 15f;
    public float returnSpeed = 4f;

    [Header("Combat Stats")]
    public int pelletCount = 8;        // Number of rays per shot
    public float spread = 0.1f;        // Max randomness of pellets
    public float range = 50f;
    public float damagePerPellet = 15f;
    public float fireRate = 0.8f;      // Slower than pistol
    
    [Header("References")]
    public Camera playerCamera;
    public AudioSource audioSource;
    public AudioClip fireSound;

    private PlayerMovement _movement;
    private Vector3 _originalPosition;
    private float _bobTimer;
    private float _nextFireTime;
    private bool _isFiring = false;

    void Start()
    {
        if (weaponDisplay != null && idleSprite != null)
        {
            weaponDisplay.sprite = idleSprite;
            _originalPosition = weaponDisplay.rectTransform.localPosition;
        }

        _movement = GetComponentInParent<PlayerMovement>();
        if (playerCamera == null) playerCamera = Camera.main;
    }

    private void OnEnable()
    {
        _isFiring = false;
        if (weaponDisplay != null && idleSprite != null)
        {
            weaponDisplay.sprite = idleSprite;
        }
    }

    void Update()
    {
        HandleWeaponBob();

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed && Time.time >= _nextFireTime && !_isFiring)
        {
            StartCoroutine(FireSequence());
        }
    }

    private void HandleWeaponBob()
    {
        if (_movement == null || weaponDisplay == null) return;

        float speed = _movement.HorizontalVelocity.magnitude;

        if (_movement.IsGrounded && speed > 0.1f)
        {
            _bobTimer += Time.deltaTime * bobSpeed;
            
            // Mathematical "Figure Eight" bobbing
            // $x = \cos(t) \cdot A_x$
            // $y = |\sin(t)| \cdot A_y$
            float xOffset = Mathf.Cos(_bobTimer) * bobAmountX;
            float yOffset = Mathf.Abs(Mathf.Sin(_bobTimer)) * bobAmountY;

            Vector3 targetBobPos = _originalPosition + new Vector3(xOffset, yOffset, 0);
            weaponDisplay.rectTransform.localPosition = Vector3.Lerp(weaponDisplay.rectTransform.localPosition, targetBobPos, Time.deltaTime * 10f);
        }
        else
        {
            _bobTimer = 0;
            weaponDisplay.rectTransform.localPosition = Vector3.Lerp(weaponDisplay.rectTransform.localPosition, _originalPosition, Time.deltaTime * returnSpeed);
        }
    }

    private IEnumerator FireSequence()
    {
        _isFiring = true;
        _nextFireTime = Time.time + fireRate;

        if (audioSource && fireSound) audioSource.PlayOneShot(fireSound);

        // --- COMBAT LOGIC: Raycast Spread ---
        ShootRaycasts();

        // Play animation
        foreach (Sprite frame in fireAnimation)
        {
            if (frame != null) weaponDisplay.sprite = frame;
            yield return new WaitForSeconds(frameRate);
        }

        weaponDisplay.sprite = idleSprite;
        _isFiring = false;
    }

    private void ShootRaycasts()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            // Calculate random spread within a sphere/cone
            Vector3 shootDir = playerCamera.transform.forward;
            shootDir.x += Random.Range(-spread, spread);
            shootDir.y += Random.Range(-spread, spread);
            shootDir.z += Random.Range(-spread, spread);

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, shootDir, out hit, range))
            {
                Debug.Log($"Shotgun hit: {hit.collider.name} with pellet {i}");
                
                // Placeholder for dealing damage to enemies
                // if (hit.collider.TryGetComponent(out EnemyHealth enemy)) enemy.TakeDamage(damagePerPellet);
            }
        }
    }
}