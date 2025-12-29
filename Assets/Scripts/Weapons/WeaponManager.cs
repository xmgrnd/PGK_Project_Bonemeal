using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

// Manages the player's inventory, weapon switching, and availability
public class WeaponManager : MonoBehaviour
{
    [Header("Inventory State")]
    public bool hasPistol = true;   // Starting with pistol for testing
    public bool hasShotgun = true; // Must be picked up

    [Header("Weapon Slots")]
    public GameObject unarmedSlot;
    public GameObject pistolSlot;
    public GameObject shotgunSlot;

    [Header("Settings")]
    public float switchDelay = 0.2f;
    private int _currentSlotIndex = 0; // 0: Unarmed, 1: Pistol, 2: Shotgun
    private bool _isSwitching = false;

    void Start()
    {
        // Initialize the starting state
        EquipWeapon(0); 
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || _isSwitching) return;

        // Key 1: Unarmed
        if (keyboard.digit1Key.wasPressedThisFrame) AttemptSwitch(0);
        
        // Key 2: Pistol
        if (keyboard.digit2Key.wasPressedThisFrame && hasPistol) AttemptSwitch(1);
        
        // Key 3: Shotgun
        if (keyboard.digit3Key.wasPressedThisFrame && hasShotgun) AttemptSwitch(2);
    }

    private void AttemptSwitch(int newIndex)
    {
        if (newIndex == _currentSlotIndex) return;
        EquipWeapon(newIndex);
    }
    
    // Forces the player to lose all weapons and stay unarmed 
    public void ClearInventoryOnDeath()
    {
        hasPistol = false;
        hasShotgun = false;
        EquipWeapon(0); // Switch to the unarmed slot visuals
    }

    public void EquipWeapon(int index)
    {
        _currentSlotIndex = index;

        // Disable all slots first
        unarmedSlot.SetActive(false);
        pistolSlot.SetActive(false);
        shotgunSlot.SetActive(false);

        // Enable the selected slot
        if (index == 0) unarmedSlot.SetActive(true);
        if (index == 1) pistolSlot.SetActive(true);
        if (index == 2) shotgunSlot.SetActive(true);

        Debug.Log($"Equipped weapon slot: {index}");
    }

    // Method to be called when picking up a new weapon
    public void UnlockShotgun()
    {
        hasShotgun = true;
        Debug.Log("Shotgun unlocked in inventory!");
    }
}