using UnityEngine;
using UnityEngine.InputSystem;

// This script controls the PSX shader parameters at runtime.
// It allows for dynamic adjustment of the vertex jittering effect.
public class PSXEffectController : MonoBehaviour
{
    public Material psxMaterial;
    public float changeSpeed = 50f;

    private float _currentRes = 120f;

    void Update()
    {
        // Using the new Input System Package to adjust the retro feel [cite: 2025-12-25]
        if (Keyboard.current == null || psxMaterial == null) return;

        // Increase jitter (Lower resolution)
        if (Keyboard.current.uKey.isPressed) 
        {
            _currentRes = Mathf.Max(10f, _currentRes - Time.deltaTime * changeSpeed);
            UpdateShader();
        }

        // Decrease jitter (Higher resolution)
        if (Keyboard.current.iKey.isPressed) 
        {
            _currentRes = Mathf.Min(256f, _currentRes + Time.deltaTime * changeSpeed);
            UpdateShader();
        }
    }

    private void UpdateShader()
    {
        psxMaterial.SetFloat("_ResolutionRescale", _currentRes);
    }
}