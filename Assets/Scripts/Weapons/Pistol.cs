using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

// Handles pistol logic, shooting, spritesheet animation, and procedural weapon bobbing
public class Pistol : MonoBehaviour
{
    [Header("Visuals (Spritesheet)")]
    public Image weaponDisplay;       
    public Sprite idleSprite;
    public Sprite[] fireAnimation;    
    public float frameRate = 0.1f;    

    [Header("Weapon Bobbing Settings")]
    // How fast the weapon bobs
    public float bobSpeed = 12f;
    // How far the weapon moves horizontally and vertically
    public float bobAmountX = 15f;
    public float bobAmountY = 10f;
    // How fast the weapon returns to center when stopping
    public float returnSpeed = 5f;

    [Header("Stats")]
    public float damage = 20f;
    public float fireRate = 0.25f;
    private float _nextFireTime;
    private bool _isFiring = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;

    // References and Internal State
    private PlayerMovement _movement;
    private Vector3 _originalPosition;
    private float _bobTimer;

    void Start()
    {
        if (weaponDisplay != null && idleSprite != null)
        {
            weaponDisplay.sprite = idleSprite;
            // Store the initial UI position
            _originalPosition = weaponDisplay.rectTransform.localPosition;
        }

        // Find movement script in parents to sync bobbing with speed
        _movement = GetComponentInParent<PlayerMovement>();
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
        HandleShooting();
        HandleWeaponBob();
    }

    private void HandleShooting()
    {
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

        // Only bob when player is grounded and moving
        if (_movement.IsGrounded && speed > 0.1f)
        {
            // Advance timer based on speed
            _bobTimer += Time.deltaTime * bobSpeed;

            // Calculate "Figure Eight" bobbing
            // Horizontal: $\cos(t)$
            // Vertical: $\sin(2t)$ for double frequency (classic FPS feel)
            float xOffset = Mathf.Cos(_bobTimer) * bobAmountX;
            float yOffset = Mathf.Abs(Mathf.Sin(_bobTimer)) * bobAmountY;

            Vector3 targetBobPos = _originalPosition + new Vector3(xOffset, yOffset, 0);
            weaponDisplay.rectTransform.localPosition = Vector3.Lerp(weaponDisplay.rectTransform.localPosition, targetBobPos, Time.deltaTime * 10f);
        }
        else
        {
            // Reset timer and smoothly return to original position when idle or in air
            _bobTimer = 0;
            weaponDisplay.rectTransform.localPosition = Vector3.Lerp(weaponDisplay.rectTransform.localPosition, _originalPosition, Time.deltaTime * returnSpeed);
        }
    }

    private IEnumerator FireSequence()
    {
        _isFiring = true;
        _nextFireTime = Time.time + fireRate;

        if (audioSource && fireSound) audioSource.PlayOneShot(fireSound);

        foreach (Sprite frame in fireAnimation)
        {
            if (frame != null) weaponDisplay.sprite = frame;
            yield return new WaitForSeconds(frameRate);
        }

        weaponDisplay.sprite = idleSprite;
        _isFiring = false;
    }
}