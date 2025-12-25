using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 15f;
    public float gravity = -20f;
    public float jumpHeight = 3f;

    Vector3 velocity;

    void Update()
    {
        // Gravity check
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Reading input from keyboard
        var keyboard = Keyboard.current;
        Vector3 move = Vector3.zero;

        if (keyboard != null)
        {
            float x = 0;
            float z = 0;

            if (keyboard.wKey.isPressed) z += 1;
            if (keyboard.sKey.isPressed) z -= 1;
            if (keyboard.aKey.isPressed) x -= 1;
            if (keyboard.dKey.isPressed) x += 1;

            move = transform.right * x + transform.forward * z;
        }

        controller.Move(move * (speed * Time.deltaTime));

        // Jump
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}