using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityScript
{
    public struct EaseValues
    {
        public bool IsFinished;             // true if ease is finished
        public float StartTime;             // the time the ease starts
        public float Duration;              // the time the ease takes
        public float StartValue;            // the value when it starts
        public float ValueChange;           // the value change

        public EaseValues(float startTime, float easeTime, float startValue, float valueChange)
        {
            IsFinished = false;
            StartTime = startTime;
            Duration = easeTime;
            StartValue = startValue;
            ValueChange = valueChange;
        }
    }

    // checks if done
    public static bool IsEaseDone(EaseValues easeValues)
    {
        return Time.timeSinceLevelLoad > easeValues.StartTime + easeValues.Duration;
    }

    public static float QuadEaseInOut(EaseValues easeValues)
    {
        return QuadEaseInOut(Time.timeSinceLevelLoad - easeValues.StartTime, easeValues.StartValue, easeValues.ValueChange, easeValues.Duration);
    }

    private static float QuadEaseInOut(float currentTime, float startValue, float valueChange, float duration)
    {
        currentTime /= duration / 2;
        if (currentTime < 1)
            return valueChange / 2 * currentTime * currentTime + startValue;
        currentTime--;
        return -valueChange / 2 * (currentTime * (currentTime - 2) - 1) + startValue;
    }

    public static float LinearTween(EaseValues easeValues)
    {
        return LinearTween(Time.timeSinceLevelLoad - easeValues.StartTime, easeValues.StartValue, easeValues.ValueChange, easeValues.Duration);
    }

    public static float LinearTween(float currentTime, float startValue, float valueChange, float duration)
    {
        currentTime /= duration;
        return valueChange * currentTime * currentTime + startValue;
    }
}

