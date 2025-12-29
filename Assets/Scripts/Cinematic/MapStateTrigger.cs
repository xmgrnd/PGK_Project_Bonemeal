using UnityEngine;

// This script coordinates game state changes, unloads terrain, manages audio, 
// and optimizes resources by disabling a list of specified objects.
// Updated to also enable the player's HUD (HP and Dash bars) [cite: 2025-12-25].
public class MapStateTrigger : MonoBehaviour
{
    [Header("1. Invisible Wall")]
    public GameObject invisibleWall;

    [Header("2. Player Components")]
    public PlayerMovement playerMovement; 
    public DashManager dashManager;       
    public WeaponManager weaponManager;   

    [Header("3. Environment & Camera")]
    public Camera playerCamera;
    public GameObject terrainObject;      

    [Header("4. Trigger Feedback")]
    public AudioClip pickupSound;

    [Header("5. Audio Management")]
    public AudioSource musicSource;   // Important: Uncheck 'Play On Awake' in Inspector
    public AudioSource ambientSource; // The forest ambient that will be turned off
    public AudioClip newMusic;        // The music track to start playing

    [Header("6. Resource Optimization")]
    // A list of GameObjects (like forests, buildings, or props) to turn off
    public GameObject[] objectsToDisable;
    
    [Header("7. Zombie Horde")]
    public ZombieSprawner spawner;
    public int initialZombieCount = 10;

    [Header("8. UI Settings")]
    // Reference to the Canvas containing HP and Dash bars [cite: 2025-12-25]
    public GameObject hudCanvas; 
    
    private void OnTriggerEnter(Collider other)
    {
        // Detect player via the Movement component
        if (other.GetComponent<PlayerMovement>() != null)
        {
            ApplyGameChanges();
            HandleAudioState();
            OptimizeResources(); 
            HidePickup();
        }
    }

    private void ApplyGameChanges()
    {
        // Play one-time sound on player's source
        if (playerMovement != null && playerMovement.audioSource != null && pickupSound != null)
        {
            playerMovement.audioSource.PlayOneShot(pickupSound);
        }

        // Enable the HUD Canvas [cite: 2025-12-25]
        if (hudCanvas != null)
        {
            hudCanvas.SetActive(true);
            Debug.Log("<color=green>UI:</color> Player HUD enabled.");
        }

        // Disable wall and terrain
        if (invisibleWall != null && invisibleWall.GetComponent<MeshCollider>() != null)
            invisibleWall.GetComponent<MeshCollider>().enabled = false;

        if (terrainObject != null) terrainObject.SetActive(false);

        // Update player stats
        if (playerMovement != null) playerMovement.maxSpeed = 10f;
        if (dashManager != null) dashManager.maxDashes = 3;

        // Visuals adjustment: Far clip plane
        if (playerCamera != null) playerCamera.farClipPlane = 1000f;

        // ATMOSPHERE ADJUSTMENTS
        RenderSettings.ambientLight = new Color(0.02f, 0.02f, 0.02f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 0f;
        RenderSettings.fogEndDistance = 30f;

        // Weapon logic: unlock and switch
        if (weaponManager != null)
        {
            weaponManager.hasPistol = true;
            weaponManager.hasShotgun = true;
            weaponManager.UnlockShotgun();
            weaponManager.EquipWeapon(2); // Forces shotgun equip
        }
        
        if (spawner != null)
        {
            spawner.StartSpawning();
        }
    }

    private void HandleAudioState()
    {
        if (ambientSource != null) ambientSource.Stop();

        if (musicSource != null && newMusic != null)
        {
            musicSource.clip = newMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    private void OptimizeResources()
    {
        if (objectsToDisable == null) return;

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    private void HidePickup()
    {
        if (GetComponent<MeshRenderer>() != null) GetComponent<MeshRenderer>().enabled = false;
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 0.5f);
    }
}