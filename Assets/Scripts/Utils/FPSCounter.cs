using UnityEngine;
using UnityEngine.InputSystem; 

public class FPSCounter : MonoBehaviour
{
    private float _deltaTime = 0.0f;
    private bool _showDisplay = false;

    void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        
        if (Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            _showDisplay = !_showDisplay;
        }
    }
    
    //fps counter upon pressing `
    void OnGUI()
    {
        if (!_showDisplay) return;

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = Color.green;
        
        float msec = _deltaTime * 1000.0f;
        float fps = 1.0f / _deltaTime;
        
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}