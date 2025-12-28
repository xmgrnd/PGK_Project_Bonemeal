using UnityEngine;
using UnityEngine.UI;

// Cycles through an array of sprites to simulate a GIF effect in UI
public class AnimatedLogo : MonoBehaviour
{
    public Image logoDisplay;
    public Sprite[] frames;
    public float framesPerSecond = 10f;

    private int _index;
    private float _timer;

    void Update()
    {
        if (frames.Length == 0 || logoDisplay == null) return;

        _timer += Time.deltaTime;

        // Change frame based on time: $t \ge \frac{1}{fps}$
        if (_timer >= 1f / framesPerSecond)
        {
            _timer = 0;
            _index = (_index + 1) % frames.Length;
            logoDisplay.sprite = frames[_index];
        }
    }
}