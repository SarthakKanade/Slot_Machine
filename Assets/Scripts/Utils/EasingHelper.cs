using UnityEngine;

namespace SlotMachine.Utils
{
    /// <summary>
    /// Pure mathematical easing equations for smooth reel deceleration, anticipation, and bounce physics.
    /// Eliminates external tweening library dependencies (zero garbage, zero allocations).
    /// </summary>
    public static class EasingHelper
    {
        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return c3 * t * t * t - c1 * t * t;
        }

        public static float EaseOutBack(float t, float overshoot = 1.70158f)
        {
            float c1 = overshoot;
            float c3 = c1 + 1f;
            float tMinusOne = t - 1f;
            return 1f + c3 * tMinusOne * tMinusOne * tMinusOne + c1 * tMinusOne * tMinusOne;
        }

        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f;
        }

        public static float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
    }
}
