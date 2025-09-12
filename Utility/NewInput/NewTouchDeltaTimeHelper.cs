using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// A singleton MonoBehaviour that tracks touch events to calculate a precise
/// deltaTime for each touch, similar to the old input system's Touch.deltaTime.
/// </summary>
public class NewTouchDeltaTimeHelper : MonoBehaviour
{
    private static NewTouchDeltaTimeHelper _instance;
    public static NewTouchDeltaTimeHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<NewTouchDeltaTimeHelper>();

                if (_instance == null)
                {
                    var go = new GameObject(nameof(NewTouchDeltaTimeHelper));
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<NewTouchDeltaTimeHelper>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    private readonly Dictionary<int, double> _touchIdToLastTime = new();
    private readonly Dictionary<int, float> _touchIdToDeltaTime = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

#if ENABLE_INPUT_SYSTEM
    private void Update()
    {
        if (Touchscreen.current == null) return;

        _touchIdToDeltaTime.Clear();

        foreach (var touch in Touchscreen.current.touches)
        {
            var phase = touch.phase.ReadValue();
            if (phase == UnityEngine.InputSystem.TouchPhase.None) continue;

            int touchId = touch.touchId.ReadValue();
            double lastTime;

            double currentUpdateTime = Touchscreen.current.lastUpdateTime;  // TODO: test that this is equal to old touch system

            if (_touchIdToLastTime.TryGetValue(touchId, out lastTime))
            {
                float deltaTime = (float)(currentUpdateTime - lastTime);
                _touchIdToDeltaTime[touchId] = deltaTime;
            }

            _touchIdToLastTime[touchId] = currentUpdateTime;

            if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                _touchIdToLastTime.Remove(touchId);
            }
        }
}
#endif

    /// <summary>
    /// Gets the calculated delta time for a specific finger for the current frame.
    /// </summary>
    /// <param name="touchId">The unique ID of the touch.</param>
    /// <returns>The calculated delta time, or 0 if this is the first frame of the touch.</returns>
    public float GetDeltaTimeForTouch(int touchId)
    {
        float deltaTime = 0.0f;
        _touchIdToDeltaTime.TryGetValue(touchId, out deltaTime);
        return deltaTime; // Returns 0.0f if not found
    }
}
