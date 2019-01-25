using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    public GameObject player;
    private float m_AnimationSpeed;                 // the speed for the death animation
    private Vector2 m_AnimationGoal;                // the spawn point to animate to
    private bool m_IsAnimationDone = true;          // true if the animation is finished

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (m_IsAnimationDone)
        {
            transform.position = player.transform.position + Vector3.back * 10;
        }
    }

    private void FixedUpdate()
    {
        Vector2 distVec = (m_AnimationGoal - (Vector2)transform.position);
        Vector2 move = distVec.normalized * m_AnimationSpeed * Time.fixedDeltaTime;
        if (move.sqrMagnitude > distVec.sqrMagnitude)
        {
            transform.position = m_AnimationGoal;
            m_IsAnimationDone = true;
        } else
        {
            transform.position += (Vector3)move;
        }
    }

    public void DeathAnimation(float animSpeed, Vector2 animGoal)
    {
        m_IsAnimationDone = false;
        m_AnimationSpeed = animSpeed;
        m_AnimationGoal = animGoal;
    }

    public bool IsAnimationDone()
    {
        return m_IsAnimationDone;
    }
}
