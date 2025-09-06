using UnityEngine;

public struct NewTouch
{
    public int fingerId;
    public Vector2 position;
    public Vector2 deltaPosition;
    public float deltaTime;
    public NewTouchPhase phase;
    public float pressure;
}

public enum NewTouchPhase
{
    None,
    Began,
    Moved,
    Ended,
    Canceled,
    Stationary,
}