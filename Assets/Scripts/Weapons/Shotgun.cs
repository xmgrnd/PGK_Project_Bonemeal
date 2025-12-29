using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

// Heavy shotgun logic with rhythmic sway and corrected projectile spread
public class Shotgun : MonoBehaviour
{
    [Header("Visuals (Spritesheet)")]
    public Image weaponDisplay;       
    public Sprite idleSprite;
    public Sprite[] fireAnimation;    
    public float frameRate = 0.08f;

    [Header("Bobbing Settings (Quake Style)")]
    public float bobSpeed = 14f; // Synchronized with HeadBob walkingBobSpeed
    public float bobAmountX = 12f;
    public float bobAmountY = 8f;
    public float bobSmoothness = 10f;

    [Header("Sway Settings")]
    public float swayAmount = 15f;    
    public float maxSwayAmount = 40f;
    public float swaySmoothness = 4f;

    [Header("Combat Stats")]
    public int pelletCount = 12; // Increased for a more powerful feel
    public float spread = 0.08f; // Corrected spread factor
    public float damagePerPellet = 10f;
    public float fireRate = 0.8f;
    public float range = 50f;
    public float recoilForce = 8f; 
    
    [Header("VFX Settings")]
    public GameObject bloodEffectPrefab; // Particle system for enemy hits
    public GameObject bulletDecalPrefab; // Impact sprite for environment

    [Header("References")]
    private MouseLook _mouseLook;
    private PlayerMovement _movement;
    public Camera playerCamera;
    public AudioSource audioSource;
    public AudioClip fireSound;
    public Image crosshairUI;       
    public Sprite shotgunCrosshair;

    private float _nextFireTime;
    private bool _isFiring = false;
    private Vector3 _originalPosition;
    private float _bobTimer;

    void Start()
    {
        _mouseLook = GetComponentInParent<MouseLook>();
        _movement = GetComponentInParent<PlayerMovement>();
        if (playerCamera == null) playerCamera = Camera.main;

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
        if (crosshairUI != null && shotgunCrosshair != null) crosshairUI.sprite = shotgunCrosshair;
    }

    private void OnDisable()
    {
        _isFiring = false;
        StopAllCoroutines();
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

        float speed = _movement.HorizontalVelocity.magnitude;
        Vector3 targetBobPos = _originalPosition;

        // Smooth Sine for Quake-style floating movement
        if (_movement.IsGrounded && speed > 0.1f)
        {
            _bobTimer += Time.deltaTime * bobSpeed;
            float bobX = Mathf.Sin(_bobTimer * 0.5f) * bobAmountX;
            float bobY = Mathf.Sin(_bobTimer) * bobAmountY;
            targetBobPos += new Vector3(bobX, bobY, 0);
        }
        else
        {
            _bobTimer = 0;
        }

        // Mouse sway logic for weight and lag feel
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float swayX = -mouseDelta.x * swayAmount * 0.1f;
        float swayY = -mouseDelta.y * swayAmount * 0.1f;
        
        swayX = Mathf.Clamp(swayX, -maxSwayAmount, maxSwayAmount);
        swayY = Mathf.Clamp(swayY, -maxSwayAmount, maxSwayAmount);
        
        Vector3 finalTarget = targetBobPos + new Vector3(swayX, swayY, 0);
        weaponDisplay.rectTransform.localPosition = Vector3.Lerp(weaponDisplay.rectTransform.localPosition, finalTarget, Time.deltaTime * bobSmoothness);
    }

    private IEnumerator FireSequence()
    {
        _isFiring = true;
        _nextFireTime = Time.time + fireRate;
        if (_mouseLook != null) _mouseLook.AddRecoil(recoilForce);
        if (audioSource && fireSound) audioSource.PlayOneShot(fireSound);

        ShootRaycasts();

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
            Vector3 shootDir = playerCamera.transform.forward + 
                               playerCamera.transform.right * Random.Range(-spread, spread) + 
                               playerCamera.transform.up * Random.Range(-spread, spread);

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, shootDir, out hit, range))
            {
                // Try to find either standard EnemyAI or the new RangedEnemyAI 
                EnemyAI enemy = hit.collider.GetComponentInParent<EnemyAI>();
                RangedEnemyAI rangedEnemy = hit.collider.GetComponentInParent<RangedEnemyAI>();

                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerPellet);
                    SpawnBlood(hit);
                }
                else if (rangedEnemy != null) 
                {
                    rangedEnemy.TakeDamage(damagePerPellet);
                    SpawnBlood(hit);
                }
                else
                {
                    SpawnImpact(hit);
                }
            }
        }
    }
    
    private void SpawnBlood(RaycastHit hit)
    {
        if (bloodEffectPrefab != null)
        {
            // Instantiate blood oriented based on hit normal
            GameObject blood = Instantiate(bloodEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(blood, 0.5f); 
        }
    }

    private void SpawnImpact(RaycastHit hit)
    {
        if (bulletDecalPrefab != null)
        {
            GameObject decal = Instantiate(bulletDecalPrefab, hit.point + hit.normal * 0.01f, Quaternion.LookRotation(hit.normal));
            Destroy(decal, 10f);
        }
    }
}