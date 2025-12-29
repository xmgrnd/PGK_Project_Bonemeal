using UnityEngine;

// Automatically destroys the VFX object after a set duration.
public class AutoDestroyVFX : MonoBehaviour
{
    public float delay = 2f;

    void Start()
    {
        // Destroy this game object to save resources
        Destroy(gameObject, delay);
    }
}