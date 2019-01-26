using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityScript
{
    public static float QuadEaseInOut(float currentTime, float startValue, float valueChange, float duration)
    {
        // t: current time, b: start value, c: change in value, d:duration
        currentTime /= duration / 2;
        if (currentTime < 1) return valueChange / 2 * currentTime * currentTime + startValue;
        currentTime--;
        return -valueChange / 2 * (currentTime * (currentTime - 2) - 1 + startValue);
    }

    public static float LinearTween(float currentTime, float startValue, float valueChange, float duration)
    {
        currentTime /= duration;
        return valueChange * currentTime * currentTime + startValue;
    }
}

