using UnityEngine;

public enum EasingType
{
    Linear,
    EaseIn,
    EaseOut,
    EaseInOut
}

public static class EasingFunctions
{
    public static float ApplyEasing(EasingType type, float t)
    {
        switch (type)
        {
            case EasingType.EaseIn:
                return t * t; // Quadratic ease-in
            case EasingType.EaseOut:
                return 1 - (1 - t) * (1 - t); // Quadratic ease-out
            case EasingType.EaseInOut:
                return t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2; // Quadratic ease-in-out
            case EasingType.Linear:
            default:
                return t; // Linear
        }
    }

    public static string ToDisplayName(this EasingType easing)
    {
        switch (easing)
        {
            case EasingType.Linear: return "Linear";
            case EasingType.EaseIn: return "Ease In";
            case EasingType.EaseOut: return "Ease out";
            case EasingType.EaseInOut: return "Ease In Out";

            default:
                UnityEngine.Debug.LogError($"Unexpected EasingType: {easing}");
                return "";
        }
    }
}
