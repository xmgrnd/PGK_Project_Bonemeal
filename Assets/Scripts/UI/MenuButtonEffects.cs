using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

// Handles UI interaction effects: shadow glow on hover and scale punch on click
public class MenuButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    [Tooltip("The TextMeshPro object used for the button label")]
    public TextMeshProUGUI tmpText;      
    [Tooltip("The AudioSource that will play UI sounds")]
    public AudioSource audioSource;      

    [Header("Audio Clips")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    [Header("Visual Settings")]
    [Tooltip("The color of the shadow when hovered")]
    public Color shadowColor = Color.yellow;
    [Tooltip("Speed of the shadow fade animation")]
    public float animationSpeed = 15f;

    [Header("Click Animation")]
    [Tooltip("Scale multiplier applied when the button is pressed down")]
    public float clickScaleDown = 0.9f; 

    private Vector3 _originalScale;
    private Color _originalShadowColor;
    private bool _isInitialized = false;

    void Start()
    {
        if (tmpText != null)
        {
            // Store original scale and shadow color for resetting later
            _originalScale = tmpText.transform.localScale;
            
            // Access the underlay color from the material properties
            _originalShadowColor = tmpText.fontMaterial.GetColor(ShaderUtilities.ID_UnderlayColor);
            _isInitialized = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isInitialized) return;
        if (audioSource && hoverSound) audioSource.PlayOneShot(hoverSound);

        // Start shadow glow animation
        StopAllCoroutines();
        StartCoroutine(AnimateShadow(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInitialized) return;
        StopAllCoroutines();
        StartCoroutine(AnimateShadow(false));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isInitialized) return;
        if (audioSource && clickSound) audioSource.PlayOneShot(clickSound);

        // Shrink text slightly to simulate a physical press
        tmpText.transform.localScale = _originalScale * clickScaleDown;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isInitialized) return;
        // Restore scale back to normal
        tmpText.transform.localScale = _originalScale;
    }

    private IEnumerator AnimateShadow(bool entering)
    {
        Color targetShadow = entering ? shadowColor : _originalShadowColor;

        // Smoothly interpolate the shadow color using Lerp
        while (ColorDistance(tmpText.fontMaterial.GetColor(ShaderUtilities.ID_UnderlayColor), targetShadow) > 0.01f)
        {
            Color currentColor = tmpText.fontMaterial.GetColor(ShaderUtilities.ID_UnderlayColor);
            tmpText.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.Lerp(currentColor, targetShadow, Time.deltaTime * animationSpeed));
            yield return null;
        }
    }

    private float ColorDistance(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b) + Mathf.Abs(c1.a - c2.a);
    }

    private void OnDisable()
    {
        // Reset visuals if the button is disabled while hovered
        if (_isInitialized)
        {
            tmpText.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, _originalShadowColor);
            tmpText.transform.localScale = _originalScale;
        }
    }
}