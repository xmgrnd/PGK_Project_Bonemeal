using UnityEngine;

// Forces the sprite to always face the camera while staying upright
public class Billboard : MonoBehaviour
{
    private Transform _camTransform;

    void Start()
    {
        if (Camera.main != null) _camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (_camTransform == null) return;

        // Calculate direction to camera
        Vector3 targetPos = _camTransform.position;
        targetPos.y = transform.position.y; // Lock vertical axis to prevent tilting

        // Make the sprite look at the camera
        transform.LookAt(targetPos);
    }
}