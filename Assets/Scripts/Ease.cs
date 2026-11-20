using UnityEngine;
using System;
using System.Collections;
public static class Ease
{
    public static float OutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float u = 1f - t;
        return 1f - (u * u * u);
    }

    public static float OutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        t = Mathf.Clamp01(t);
        float u = t - 1f;
        return 1f + c3 * u * u * u + c1 * u * u;
    }

    public static IEnumerator Tween(
        float duration,
        Action<float> apply,
        Func<float,float> ease,
        bool unscaled =true)
    {
        if (duration <= 0f)
        {
            apply?.Invoke(1f); yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed/duration);
            float eased = (ease != null) ? ease(t) : t;
            apply?.Invoke(eased); 
            yield return null;
        }
        apply?.Invoke(1f);
    }
}
