using UnityEngine;
using UnityEngine.UI;

// Sets a default crosshair when the player is not carrying any weapon
public class UnarmedWeapon : MonoBehaviour
{
    [Header("Crosshair Settings")]
    public Image crosshairUI;      // Reference to the Canvas Image
    public Sprite defaultCrosshair; // Small dot or empty sprite for unarmed state

    private void OnEnable()
    {
        // Reset the crosshair to the default "unarmed" version immediately
        if (crosshairUI != null && defaultCrosshair != null)
        {
            crosshairUI.sprite = defaultCrosshair;
            crosshairUI.rectTransform.localScale = Vector3.one; // Reset scale to 1:1
        }
    }
}