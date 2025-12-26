using UnityEngine;

// This script deals damage to the player when they enter the trigger zone
public class DamageZone : MonoBehaviour
{
    [Header("Damage Settings")]
    // Amount of HP to subtract from the player
    public float damageAmount = 15f;
    
    // Cooldown to prevent dealing damage every single frame
    public float damageInterval = 1.0f;
    private float _nextDamageTime;

    // Triggered when another collider enters this object's trigger space
    private void OnTriggerStay(Collider other)
    {
        // Check if the object entering has the PlayerHealth component
        // We look in the parent in case the collider is on a child object
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null && Time.time >= _nextDamageTime)
        {
            // Apply the damage using the method we created earlier
            health.TakeDamage(damageAmount);
            
            // Set the next allowed damage time: $t_{next} = t_{current} + t_{interval}$
            _nextDamageTime = Time.time + damageInterval;
            
            Debug.Log($"DamageZone: Dealt {damageAmount} damage to Player.");
        }
    }
}