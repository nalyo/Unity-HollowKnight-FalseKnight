using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JumpMotionHelper
{
    public static float GetClipLength(string name, Animator anim, float fallback)
    {
        if (anim == null || anim.runtimeAnimatorController == null) return fallback;
        foreach (var c in anim.runtimeAnimatorController.animationClips)
            if (c.name == name) return c.length;
        return fallback;
    }

    public static Vector3 GetHorizVel(Vector3 start, Vector3 end, float totalTime)
    {
        return new Vector3(end.x - start.x, 0f, end.z - start.z) / Mathf.Max(0.0001f, totalTime);
    }

    public static float GetInitV0y(Vector3 start, Vector3 end, float totalTime, float ascentBoost)
    {
        float g = Physics.gravity.y;
        float sY = end.y - start.y;
        float v0_physical = (sY - 0.5f * g * totalTime * totalTime) / Mathf.Max(0.0001f, totalTime);
        return v0_physical * ascentBoost;
    }

    public static float IntegrateYOffset(
        ref float currentVy,
        float deltaTime,
        float gravityY,
        float gravityUpScale,
        float gravityDownScale)
    {
        // 根据当前速度方向决定用哪个重力
        float gEffective = currentVy > 0 ? gravityY : gravityY * gravityDownScale;

        // 更新速度
        currentVy += gEffective * deltaTime;

        // 返回位移增量
        return currentVy * deltaTime;
    }

}

public static class ActionHelper
{
    public static T WeightedRandom<T>(T[] items, float[] weights)
    {
        float total = 0;
        foreach (float w in weights) total += w;

        float rand = Random.Range(0, total);
        float cumulative = 0;

        for (int i = 0; i < items.Length; i++)
        {
            cumulative += weights[i];
            if (rand < cumulative)
                return items[i];
        }
        return items[items.Length - 1];
    }

}

public static class Vector3Extensions
{
    public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max)
    {
        return new Vector3(
            Mathf.Clamp(value.x, min.x, max.x),
            Mathf.Clamp(value.y, min.y, max.y),
            Mathf.Clamp(value.z, min.z, max.z)
        );
    }
}
