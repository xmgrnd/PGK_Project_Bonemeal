using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

// Comprehensive pistol logic with Quake-style bobbing and mouse sway
public class Pistol : MonoBehaviour
{
    [Header("Visuals (Spritesheet)")] 
    public Image weaponDisplay;
    public Sprite idleSprite;
    public Sprite[] fireAnimation;
    public float frameRate = 0.1f;

    [Header("Bobbing Settings (Quake Style)")]
    public float bobSpeed = 12f;      
    public float bobAmountX = 15f;    
    public float bobAmountY = 10f;    
    public float bobSmoothness = 10f; 

    [Header("Sway Settings")]
    public float swayAmount = 20f;    // How much the gun lags behind mouse movement
    public float maxSwayAmount = 50f; // Limit to prevent the gun from leaving the screen
    public float swaySmoothness = 5f; 

    [Header("Combat Stats")] 
    public float damage = 20f;
    public float fireRate = 0.25f;
    public float recoilForce = 2f;
    private float _nextFireTime;
    private bool _isFiring = false;

    [Header("References")] 
    private MouseLook _mouseLook;
    private PlayerMovement _movement;
    private Vector3 _originalPosition;
    private float _bobTimer;

    [Header("Audio & UI")] 
    public AudioSource audioSource;
    public AudioClip fireSound;
    public Image crosshairUI;
    public Sprite pistolCrosshair;

    [Header("VFX Settings")] 
    public GameObject bloodEffectPrefab;

    void Start()
    {
        _mouseLook = GetComponentInParent<MouseLook>();
        _movement = GetComponentInParent<PlayerMovement>();

        if (weaponDisplay != null && idleSprite != null)
        {
            weaponDisplay.sprite = idleSprite;
            _originalPosition = weaponDisplay.rectTransform.localPosition;
        }
    }

    private void OnEnable()
    {
        _isFiring = false;
        if (weaponDisplay != null && idleSprite != null) weaponDisplay.sprite = idleSprite;
        if (crosshairUI != null && pistolCrosshair != null) crosshairUI.sprite = pistolCrosshair;
    }

    void Update()
    {
        HandleFiring();
        HandleWeaponVisuals();
    }

    private void HandleFiring()
    {
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed && Time.time >= _nextFireTime && !_isFiring)
        {
            StartCoroutine(FireSequence());
        }
    }

    private void HandleWeaponVisuals()
    {
        if (_movement == null || weaponDisplay == null) return;

        // --- 1. BOBBING LOGIC (Walking) ---
        float speed = _movement.HorizontalVelocity.magnitude;
        Vector3 targetBobPos = _originalPosition;

        if (_movement.IsGrounded && speed > 0.1f)
        {
            _bobTimer += Time.deltaTime * bobSpeed;
            // Smooth Sine instead of Absolute Sine for the "floating" Quake effect
            float bobX = Mathf.Sin(_bobTimer * 0.5f) * bobAmountX;
            float bobY = Mathf.Sin(_bobTimer) * bobAmountY;
            targetBobPos += new Vector3(bobX, bobY, 0);
        }
        else
        {
            _bobTimer = 0;
        }

        // --- 2. SWAY LOGIC (Mouse Movement) ---
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float swayX = -mouseDelta.x * swayAmount * 0.1f;
        float swayY = -mouseDelta.y * swayAmount * 0.1f;
        
        // Clamp the sway so the gun doesn't fly off-screen
        swayX = Mathf.Clamp(swayX, -maxSwayAmount, maxSwayAmount);
        swayY = Mathf.Clamp(swayY, -maxSwayAmount, maxSwayAmount);
        
        Vector3 targetSwayPos = new Vector3(swayX, swayY, 0);

        // --- 3. APPLY FINAL POSITION ---
        // Combine both Bobbing and Sway for the final feel
        Vector3 finalTarget = targetBobPos + targetSwayPos;
        weaponDisplay.rectTransform.localPosition = Vector3.Lerp(weaponDisplay.rectTransform.localPosition, finalTarget, Time.deltaTime * bobSmoothness);
    }

    private void OnDisable()
    {
        _isFiring = false;
        StopAllCoroutines();
    }

    private IEnumerator FireSequence()
    {
        _isFiring = true;
        _nextFireTime = Time.time + fireRate;
        if (_mouseLook != null) _mouseLook.AddRecoil(recoilForce);
        if (audioSource && fireSound) audioSource.PlayOneShot(fireSound);

        RaycastHit hit;
        // Standard raycast from the center of the camera forward
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 100f))
        {
            // Check for both types of enemies [cite: 2025-12-25]
            EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
            RangedEnemyAI rangedEnemy = hit.collider.GetComponentInParent<RangedEnemyAI>();

            if (enemy != null) 
            { 
                enemy.TakeDamage(damage); 
                SpawnBlood(hit); 
            }
            else if (rangedEnemy != null) // Logic for the new ranged enemy type [cite: 2025-12-25]
            {
                rangedEnemy.TakeDamage(damage);
                SpawnBlood(hit);
            }
        }

        foreach (Sprite frame in fireAnimation)
        {
            if (frame != null) weaponDisplay.sprite = frame;
            yield return new WaitForSeconds(frameRate);
        }

        weaponDisplay.sprite = idleSprite;
        _isFiring = false;
    }
    
    private void SpawnBlood(RaycastHit hit)
    {
        if (bloodEffectPrefab != null)
        {
            GameObject blood = Instantiate(bloodEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(blood, 0.5f); 
        }
    }
}