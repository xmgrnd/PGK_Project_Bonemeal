using UnityEngine;

// Forces the object to face the camera while staying upright.
// Includes a fix for mirrored/backwards text common in TextMeshPro.
public class BillboardText : MonoBehaviour
{
    [Header("Settings")]
    public bool flipText = true; // Use this for TextMeshPro objects
    private Transform _camTransform;

    void Start()
    {
        // Cache the main camera transform for performance
        if (Camera.main != null) 
            _camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (_camTransform == null) return;

        // Calculate direction to camera and lock the Y axis to keep it upright
        Vector3 targetPos = _camTransform.position;
        targetPos.y = transform.position.y; 

        // Make the sprite/text look at the camera
        transform.LookAt(targetPos);

        // If it's a TextMeshPro object, it usually needs a 180-degree correction
        if (flipText)
        {
            transform.Rotate(0, 180, 0);
        }
    }
}