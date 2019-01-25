using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public GameObject player;

    enum CameraState { Normal, Ragdoll, DeathAnimation}

    private CameraState m_CameraState = CameraState.Normal;

    public float m_DeathAnimTime = 0.5f;
    public float m_RagdollZoom = 0.3f;
    public float m_RagdollZoomTime = 1f;


    private Vector2 m_DeathAnimStartValue;
    private Vector2 m_DeathAnimValueChange;

    private float m_RagdollAnimStartValue;
    private float m_RagdollAnimValueChange;

    private float m_AnimationStartTime;

    private bool m_IsAnimationDone = true;          // true if the animation is finished


    // Start is called before the first frame update
    void Start()
    {
        // add listener for ragdoll animation
        // GameObject.Find("Level").GetComponent<LevelManager>().OnRagdollEvent.AddListener(InitRagdollAnimation);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_CameraState == CameraState.DeathAnimation)
        {
            if (!m_IsAnimationDone)
            {
                DeathAnimation();
            }
        }
        else if (m_CameraState == CameraState.Ragdoll)
        {
            RagdollAnimation();
        }
        else if (m_CameraState == CameraState.Normal)
        {
            transform.position = player.transform.position + Vector3.back * 10;
        }
    }

    private void DeathAnimation()
    {
        float value = QuadEaseInOut(Time.timeSinceLevelLoad - m_AnimationStartTime, 0, 1, m_RagdollZoomTime);
        transform.position = (Vector3)(m_DeathAnimStartValue + value * m_DeathAnimValueChange) + Vector3.back * 10;
        if (value > 0.98)
        {
            m_IsAnimationDone = true;
            m_CameraState = CameraState.Normal;
        }
    }

    private void RagdollAnimation()
    {
        float value = QuadEaseInOut(Time.timeSinceLevelLoad - m_AnimationStartTime, m_RagdollAnimStartValue, m_RagdollAnimValueChange, m_RagdollZoomTime);
        gameObject.GetComponent<Camera>().orthographicSize = value;
        transform.position = player.transform.position + Vector3.back * 10;
    }

    public void InitDeathAnimation(Vector2 animGoal)
    {
        m_IsAnimationDone = false;
        m_DeathAnimStartValue = (Vector2)transform.position;
        m_DeathAnimValueChange = animGoal - (Vector2)transform.position;
        m_CameraState = CameraState.DeathAnimation;
        m_AnimationStartTime = Time.timeSinceLevelLoad;
    }

    private void InitRagdollAnimation()
    {
        m_RagdollAnimStartValue = gameObject.GetComponent<RectTransform>().localScale.x;
        m_RagdollAnimValueChange = m_RagdollZoom - m_RagdollAnimStartValue;
        m_CameraState = CameraState.Ragdoll;
        m_AnimationStartTime = Time.timeSinceLevelLoad;
    }

    public bool IsAnimationDone()
    {
        return m_IsAnimationDone;
    }

    public float QuadEaseInOut(float t, float b, float c, float d)
    {
        // t: current time, b: start value, c: change in value, d:duration
        t /= d/2;
        if (t < 1) return c / 2 * t * t + b;
        t--;
        return -c / 2 * (t * (t - 2) - 1 + b);
    }
}
