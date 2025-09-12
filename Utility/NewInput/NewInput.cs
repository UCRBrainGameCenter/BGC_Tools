using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BGC.Utility
{
    /// <summary>
    /// Converts from the old input system <see cref="KeyCode"/> to the new input system <see cref="Key"/>.
    /// </summary>
    public static class NewInput
    {
#if ENABLE_INPUT_SYSTEM
        private static readonly Dictionary<KeyCode, Key> keyMapping = new()
        {
            { KeyCode.None, Key.None },
            { KeyCode.Backspace, Key.Backspace },
            { KeyCode.Delete, Key.Delete },
            { KeyCode.Tab, Key.Tab },
            { KeyCode.Clear, Key.None },
            { KeyCode.Return, Key.Enter },
            { KeyCode.Pause, Key.Pause },
            { KeyCode.Escape, Key.Escape },
            { KeyCode.Space, Key.Space },

            { KeyCode.Keypad0, Key.Numpad0 },
            { KeyCode.Keypad1, Key.Numpad1 },
            { KeyCode.Keypad2, Key.Numpad2 },
            { KeyCode.Keypad3, Key.Numpad3 },
            { KeyCode.Keypad4, Key.Numpad4 },
            { KeyCode.Keypad5, Key.Numpad5 },
            { KeyCode.Keypad6, Key.Numpad6 },
            { KeyCode.Keypad7, Key.Numpad7 },
            { KeyCode.Keypad8, Key.Numpad8 },
            { KeyCode.Keypad9, Key.Numpad9 },
            { KeyCode.KeypadPeriod, Key.NumpadPeriod },
            { KeyCode.KeypadDivide, Key.NumpadDivide },
            { KeyCode.KeypadMultiply, Key.NumpadMultiply },
            { KeyCode.KeypadMinus, Key.NumpadMinus },
            { KeyCode.KeypadPlus, Key.NumpadPlus },
            { KeyCode.KeypadEnter, Key.NumpadEnter },
            { KeyCode.KeypadEquals, Key.NumpadEquals },

            { KeyCode.UpArrow, Key.UpArrow },
            { KeyCode.DownArrow, Key.DownArrow },
            { KeyCode.RightArrow, Key.RightArrow },
            { KeyCode.LeftArrow, Key.LeftArrow },
            { KeyCode.Insert, Key.Insert },
            { KeyCode.Home, Key.Home },
            { KeyCode.End, Key.End },
            { KeyCode.PageUp, Key.PageUp },
            { KeyCode.PageDown, Key.PageDown },

            { KeyCode.F1, Key.F1 },
            { KeyCode.F2, Key.F2 },
            { KeyCode.F3, Key.F3 },
            { KeyCode.F4, Key.F4 },
            { KeyCode.F5, Key.F5 },
            { KeyCode.F6, Key.F6 },
            { KeyCode.F7, Key.F7 },
            { KeyCode.F8, Key.F8 },
            { KeyCode.F9, Key.F9 },
            { KeyCode.F10, Key.F10 },
            { KeyCode.F11, Key.F11 },
            { KeyCode.F12, Key.F12 },
            { KeyCode.F13, Key.None },
            { KeyCode.F14, Key.None },
            { KeyCode.F15, Key.None },

            { KeyCode.Alpha0, Key.Digit0 },
            { KeyCode.Alpha1, Key.Digit1 },
            { KeyCode.Alpha2, Key.Digit2 },
            { KeyCode.Alpha3, Key.Digit3 },
            { KeyCode.Alpha4, Key.Digit4 },
            { KeyCode.Alpha5, Key.Digit5 },
            { KeyCode.Alpha6, Key.Digit6 },
            { KeyCode.Alpha7, Key.Digit7 },
            { KeyCode.Alpha8, Key.Digit8 },
            { KeyCode.Alpha9, Key.Digit9 },

            { KeyCode.Exclaim, Key.None },
            { KeyCode.DoubleQuote, Key.None },
            { KeyCode.Hash, Key.None },
            { KeyCode.Dollar, Key.None },
            { KeyCode.Percent, Key.None },
            { KeyCode.Ampersand, Key.None },
            { KeyCode.Quote, Key.Quote },
            { KeyCode.LeftParen, Key.None },
            { KeyCode.RightParen, Key.None },
            { KeyCode.Asterisk, Key.None },
            { KeyCode.Plus, Key.None },
            { KeyCode.Comma, Key.Comma },
            { KeyCode.Minus, Key.Minus },
            { KeyCode.Period, Key.Period },
            { KeyCode.Slash, Key.Slash },
            { KeyCode.Colon, Key.None },
            { KeyCode.Semicolon, Key.Semicolon },
            { KeyCode.Less, Key.None },
            { KeyCode.Equals, Key.Equals },
            { KeyCode.Greater, Key.None },
            { KeyCode.Question, Key.None },
            { KeyCode.At, Key.None },

            { KeyCode.LeftBracket, Key.LeftBracket },
            { KeyCode.Backslash, Key.Backslash },
            { KeyCode.RightBracket, Key.RightBracket },
            { KeyCode.Caret, Key.None },
            { KeyCode.Underscore, Key.None },
            { KeyCode.BackQuote, Key.Backquote },

            { KeyCode.A, Key.A },
            { KeyCode.B, Key.B },
            { KeyCode.C, Key.C },
            { KeyCode.D, Key.D },
            { KeyCode.E, Key.E },
            { KeyCode.F, Key.F },
            { KeyCode.G, Key.G },
            { KeyCode.H, Key.H },
            { KeyCode.I, Key.I },
            { KeyCode.J, Key.J },
            { KeyCode.K, Key.K },
            { KeyCode.L, Key.L },
            { KeyCode.M, Key.M },
            { KeyCode.N, Key.N },
            { KeyCode.O, Key.O },
            { KeyCode.P, Key.P },
            { KeyCode.Q, Key.Q },
            { KeyCode.R, Key.R },
            { KeyCode.S, Key.S },
            { KeyCode.T, Key.T },
            { KeyCode.U, Key.U },
            { KeyCode.V, Key.V },
            { KeyCode.W, Key.W },
            { KeyCode.X, Key.X },
            { KeyCode.Y, Key.Y },
            { KeyCode.Z, Key.Z },

            { KeyCode.Numlock, Key.NumLock },
            { KeyCode.CapsLock, Key.CapsLock },
            { KeyCode.ScrollLock, Key.ScrollLock },
            { KeyCode.RightShift, Key.RightShift },
            { KeyCode.LeftShift, Key.LeftShift },
            { KeyCode.RightControl, Key.RightCtrl },
            { KeyCode.LeftControl, Key.LeftCtrl },
            { KeyCode.RightAlt, Key.RightAlt },
            { KeyCode.LeftAlt, Key.LeftAlt },
            { KeyCode.RightCommand, Key.RightCommand },
            { KeyCode.LeftCommand, Key.LeftCommand },
            { KeyCode.LeftWindows, Key.LeftWindows },
            { KeyCode.RightWindows, Key.RightWindows },
            { KeyCode.AltGr, Key.AltGr },

            { KeyCode.Help, Key.None },
            { KeyCode.Print, Key.PrintScreen },
            { KeyCode.SysReq, Key.None },
            { KeyCode.Break, Key.None },
            { KeyCode.Menu, Key.ContextMenu },
        };
        public static readonly NewGyroscope gyro = new NewGyroscope();
#endif

        /// <summary>
        /// Converts <see cref="Input.touchCount"/> to the new input system
        /// </summary>
        public static int TouchCount =>
#if ENABLE_INPUT_SYSTEM
            CountInProgressTouches();
#else
            Input.touchCount;
#endif

#if ENABLE_INPUT_SYSTEM
        private static int CountInProgressTouches()
        {
            if (Touchscreen.current == null) return 0;

            int count = 0;
            foreach (var t in Touchscreen.current.touches)
            {
                var phase = t.phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began ||
                    phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                    phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    count++;
                }
            }
            return count;
        }
#endif

        /// <summary>
        /// Converts <see cref="Input.anyKeyDown"/> to the new input system
        /// </summary>
        public static bool AnyKeyDown =>
#if ENABLE_INPUT_SYSTEM
            Keyboard.current.anyKey.wasPressedThisFrame ||
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame;
#else
            Input.anyKeyDown;
#endif

        /// <summary>
        /// Converts <see cref="Input.anyKey"/> to the new input system
        /// </summary>
        public static bool AnyKey =>
#if ENABLE_INPUT_SYSTEM
            Keyboard.current.anyKey.isPressed ||
            Mouse.current.leftButton.isPressed ||
            Mouse.current.rightButton.isPressed ||
            Mouse.current.middleButton.isPressed;
#else
            Input.anyKey;
#endif

        /// <summary>
        /// Converts <see cref="Input.touches"/> to the new input system
        /// </summary>
        public static List<NewTouch> Touches => GetTouches();

        /// <summary>
        /// Returns object representing status of a specific touch, following
        /// the same behavior as <see cref="UnityEngine.Input.GetTouch(int)"/>.
        /// </summary>
        public static NewTouch GetTouch(int index)
        {
            var touches = Touches;

            if (index < 0 || index >= touches.Count)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(index),
                    $"Invalid touch index {index}. Valid range is [0..{touches.Count - 1}]."
                );
            }

            return touches[index];
        }


        public static List<NewTouch> GetTouches()
        {
            List<NewTouch> touches = new();
#if ENABLE_INPUT_SYSTEM
            foreach (var t in Touchscreen.current.touches)
            {
                touches.Add(new()
                {
                    fingerId = t.touchId.ReadValue(),
                    position = t.position.ReadValue(),
                    deltaPosition = t.delta.ReadValue(),
                    deltaTime = NewTouchDeltaTimeHelper.Instance.GetDeltaTimeForTouch(t.touchId.ReadValue()),
                    phase = ConvertPhase(t.phase.ReadValue()),
                    pressure = t.pressure.ReadValue(),
                });
            }
#else
            foreach (var t in Input.touches)
            {
                touches.Add(new()
                {
                    fingerId = t.fingerId,
                    position = t.position,
                    deltaPosition = t.deltaPosition,
                    deltaTime = t.deltaTime,
                    phase = ConvertPhase(t.phase),
                    pressure = t.pressure
                });
            }
#endif
            return touches;
        }

#if ENABLE_INPUT_SYSTEM
        private static NewTouchPhase ConvertPhase(UnityEngine.InputSystem.TouchPhase phase)
        {
            return phase switch
            {
                UnityEngine.InputSystem.TouchPhase.None => NewTouchPhase.None,
                UnityEngine.InputSystem.TouchPhase.Began => NewTouchPhase.Began,
                UnityEngine.InputSystem.TouchPhase.Moved => NewTouchPhase.Moved,
                UnityEngine.InputSystem.TouchPhase.Stationary => NewTouchPhase.Stationary,
                UnityEngine.InputSystem.TouchPhase.Ended => NewTouchPhase.Ended,
                UnityEngine.InputSystem.TouchPhase.Canceled => NewTouchPhase.Canceled,
                _ => NewTouchPhase.Canceled
            };
        }
#else
        private static NewTouchPhase ConvertPhase(UnityEngine.TouchPhase phase)
        {
            return phase switch
            {
                UnityEngine.TouchPhase.Began => NewTouchPhase.Began,
                UnityEngine.TouchPhase.Moved => NewTouchPhase.Moved,
                UnityEngine.TouchPhase.Stationary => NewTouchPhase.Stationary,
                UnityEngine.TouchPhase.Ended => NewTouchPhase.Ended,
                UnityEngine.TouchPhase.Canceled => NewTouchPhase.Canceled,
                _ => NewTouchPhase.Canceled
            };
        }
#endif

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Converts a <see cref="KeyCode"/> to its corresponding <see cref="Key"/>.
        /// </summary>
        /// <param name="keyCode">The <see cref="KeyCode"/> to convert.</param>
        /// <returns>The corresponding <see cref="Key"/>, or <see cref="Key.None"/> if no mapping is found.</returns>
        public static Key ToKey(KeyCode keyCode)
        {
            if (keyMapping.TryGetValue(keyCode, out Key key))
            {
                return key;
            }

            Debug.LogWarning($"No mapping found for KeyCode: {keyCode}. Returning Key.None.");
            return Key.None;
        }
#endif

        /// <summary>
        /// Returns true during the frame the user starts pressing down the key identified by the <see cref="KeyCode"/> enum parameter.
        /// </summary>
        public static bool GetKeyDown(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            return GetKeyDown(ToKey(keyCode));
#else
            return Input.GetKeyDown(keyCode);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Returns true during the frame the user starts pressing down the key identified by the <see cref="Key"/> enum parameter.
        /// </summary>
        public static bool GetKeyDown(Key key)
        {
            if (Keyboard.current == null || key == Key.None)
            {
                return false;
            }
            return Keyboard.current[key].wasPressedThisFrame;
        }
#endif

        /// <summary>
        /// Returns true while the user holds down the key identified by the <see cref="KeyCode"/> enum parameter.
        /// </summary>
        public static bool GetKey(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            return GetKey(ToKey(keyCode));
#else
            return Input.GetKey(keyCode);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Returns true while the user holds down the key identified by the <see cref="Key"/> enum parameter.
        /// </summary>
        public static bool GetKey(Key key)
        {
            if (Keyboard.current == null || key == Key.None)
            {
                return false;
            }
            return Keyboard.current[key].isPressed;
        }
#endif

        /// <summary>
        /// Returns true during the frame the user releases the key identified by the <see cref="KeyCode"/> enum parameter.
        /// </summary>
        public static bool GetKeyUp(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            return GetKeyUp(ToKey(keyCode));
#else
            return Input.GetKeyUp(keyCode);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Returns true during the frame the user releases the key identified by the <see cref="Key"/> enum parameter.
        /// </summary>
        public static bool GetKeyUp(Key key)
        {
            if (Keyboard.current == null || key == Key.None)
            {
                return false;
            }
            return Keyboard.current[key].wasReleasedThisFrame;
        }
#endif

        /// <summary>
        /// Gets the current mouse position.
        /// </summary>
        public static Vector2 MousePosition
#if ENABLE_INPUT_SYSTEM
            => Mouse.current == null ? Vector2.zero : Mouse.current.position.ReadValue();
#else
            => Input.mousePosition;
#endif

        /// <summary>
        /// Returns true during the frame the user presses the mouse button identified by the button parameter.
        /// </summary>
        public static bool GetMouseButtonDown(int button)
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && button switch
            {
                0 => Mouse.current.leftButton.wasPressedThisFrame,
                1 => Mouse.current.rightButton.wasPressedThisFrame,
                2 => Mouse.current.middleButton.wasPressedThisFrame,
                _ => false,
            };
#else
            return Input.GetMouseButtonDown(button);
#endif
        }

        /// <summary>
        /// Returns true while the user holds down the mouse button identified by the button parameter.
        /// </summary>
        public static bool GetMouseButton(int button)
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && button switch
            {
                0 => Mouse.current.leftButton.isPressed,
                1 => Mouse.current.rightButton.isPressed,
                2 => Mouse.current.middleButton.isPressed,
                _ => false,
            };
#else
            return Input.GetMouseButton(button);
#endif
        }

        /// <summary>
        /// Returns true during the frame the user releases the mouse button identified by the button parameter.
        /// </summary>
        public static bool GetMouseButtonUp(int button)
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && button switch
            {
                0 => Mouse.current.leftButton.wasReleasedThisFrame,
                1 => Mouse.current.rightButton.wasReleasedThisFrame,
                2 => Mouse.current.middleButton.wasReleasedThisFrame,
                _ => false,
            };
#else
            return Input.GetMouseButtonUp(button);
#endif
        }

        /// <summary>
        /// Returns the raw value of a virtual axis (“Horizontal”, “Vertical”, “Mouse X” or “Mouse Y”)
        /// without any smoothing. Uses new Input System if ENABLE_INPUT_SYSTEM is defined.
        /// </summary>
        public static float GetAxisRaw(string axisName)
        {
#if ENABLE_INPUT_SYSTEM
            switch (axisName)
            {
                case "Horizontal":
                    {
                        // 1. Try gamepad left stick
                        float value = Gamepad.current?.leftStick.x.ReadValue() ?? 0f;

                        // 2. If stick is centered, fall back to keyboard
                        if (Mathf.Approximately(value, 0f) && Keyboard.current != null)
                        {
                            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                            {
                                value = -1f;
                            }
                            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                            {
                                value = 1f;
                            }
                        }
                        return value;
                    }

                case "Vertical":
                    {
                        float value = Gamepad.current?.leftStick.y.ReadValue() ?? 0f;

                        if (Mathf.Approximately(value, 0f) && Keyboard.current != null)
                        {
                            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                            {
                                value = -1f;
                            }
                            else if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                            {
                                value = 1f;
                            }
                        }
                        return value;
                    }

                case "Mouse X":
                    return Mouse.current != null
                        ? Mouse.current.delta.x.ReadValue()
                        : 0f;

                case "Mouse Y":
                    return Mouse.current != null
                        ? Mouse.current.delta.y.ReadValue()
                        : 0f;

                default:
                    return 0f;
            }
#else
            // old Input Manager fallback
            return Input.GetAxisRaw(axisName);
#endif
        }

            /// <summary>
            /// Returns the smoothed value of a virtual axis (“Horizontal”, “Vertical”, “Mouse X” or “Mouse Y”)
            /// similar to <see cref="Input.GetAxis"/>. Uses new Input System if ENABLE_INPUT_SYSTEM is defined.
            /// </summary>
public static float GetAxis(string axisName)
        {
#if ENABLE_INPUT_SYSTEM
            // Tune these to taste to approximate your old Input Manager settings.
            const float stickSensitivity = 10f; // how fast we reach the target when there is input
            const float stickGravity = 3f;  // how fast we fall back to 0 when input stops
            const float mouseSensitivity = 20f; // smoothing for mouse deltas
            const float mouseGravity = 20f; // decay of mouse response toward 0

            // Use your raw mapping as the target, then smooth toward it.
            float target = GetAxisRaw(axisName);

            switch (axisName)
            {
                case "Horizontal":
                    return SmoothAxis(axisName, target, stickSensitivity, stickGravity);

                case "Vertical":
                    return SmoothAxis(axisName, target, stickSensitivity, stickGravity);

                case "Mouse X":
                    return SmoothAxis(axisName, target, mouseSensitivity, mouseGravity);

                case "Mouse Y":
                    return SmoothAxis(axisName, target, mouseSensitivity, mouseGravity);

                default:
                    return 0f;
            }
#else
    // old Input Manager fallback
    return Input.GetAxis(axisName);
#endif
        }

#if ENABLE_INPUT_SYSTEM
        // Backing storage for smoothed values per axis.
        private static readonly System.Collections.Generic.Dictionary<string, float> _axisValues
            = new System.Collections.Generic.Dictionary<string, float>();

        private static float SmoothAxis(string name, float target, float sensitivity, float gravity)
        {
            float current = 0f;
            if (!_axisValues.TryGetValue(name, out current))
                _axisValues[name] = 0f;

            // Move toward the target at 'sensitivity' when there is input,
            // otherwise decay toward 0 at 'gravity'.
            float rate = (Mathf.Approximately(target, 0f) ? gravity : sensitivity) * Time.deltaTime;
            float next = Mathf.MoveTowards(current, target, rate);
            _axisValues[name] = next;
            return next;
        }
#endif
        /// <summary>
        /// Returns the device acceleration vector. Equivalent to <see cref="UnityEngine.Input.acceleration"/>.
        /// </summary>
        public static Vector3 Acceleration
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                if (Accelerometer.current != null)
                {
                    return Accelerometer.current.acceleration.ReadValue();
                }
                return Vector3.zero;
#else
        return Input.acceleration;
#endif
            }
        }

    }
}
