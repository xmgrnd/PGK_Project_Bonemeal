using UnityEngine;
using UnityEngine.InputSystem;

// This script plays a sprite animation from a spritesheet on a 2D object.
// It allows manual control over the animation speed and frame updates.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSheetAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] animationFrames;
    public float framesPerSecond = 10f;
    public bool loop = true;

    private SpriteRenderer _spriteRenderer;
    private int _currentFrame;
    private float _timer;
    private bool _isPlaying = true;

    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // New Input System: Press 'P' to pause/play the animation 
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            _isPlaying = !_isPlaying;
            Debug.Log($"<color=cyan>Animation:</color> Playing state: {_isPlaying}");
        }

        if (_isPlaying)
        {
            UpdateAnimation();
        }
    }

    private void UpdateAnimation()
    {
        if (animationFrames.Length == 0) return;

        _timer += Time.deltaTime;

        // Formula to calculate the current frame based on time:
        // CurrentFrame = floor(Time * FPS) % FrameCount
        float interval = 1f / framesPerSecond;

        if (_timer >= interval)
        {
            _timer -= interval;
            _currentFrame++;

            if (_currentFrame >= animationFrames.Length)
            {
                if (loop) _currentFrame = 0;
                else _currentFrame = animationFrames.Length - 1;
            }

            _spriteRenderer.sprite = animationFrames[_currentFrame];
        }
    }
}