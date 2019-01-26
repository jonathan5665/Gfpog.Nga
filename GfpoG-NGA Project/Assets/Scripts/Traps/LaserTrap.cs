using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    [Range(0, 5000)] [SerializeField] private int m_ShootDelayMS = 1000;
    [Range(0, 5000)] [SerializeField] private int m_ShootTimeMS = 2000;  // the speed of the projectile
    [SerializeField] private LineRenderer m_LineRenderer;


    private float m_NextTime;   // the time for the next change
    private bool m_Shooting = false;

    private void FixedUpdate()
    {
        // if over time switch
        if (Time.timeSinceLevelLoad > m_NextTime)
        {
            m_Shooting = !m_Shooting;
            if (m_Shooting)
            {
                m_NextTime = Time.timeSinceLevelLoad + m_ShootTimeMS;
            } else
            {
                m_NextTime = Time.timeSinceLevelLoad + m_ShootDelayMS;
            }
        }

        if (m_Shooting)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Vector2 fire_pos = transform.position - transform.up;

        RaycastHit2D hitInfo = Physics2D.Raycast(transform.position - transform.up, -transform.up);

        Debug.Log(m_LineRenderer);


        if (hitInfo)
        {
            CharacterController2D player = hitInfo.transform.GetComponent<CharacterController2D>();

            if (player != null)
                player.Kill();


            m_LineRenderer.SetPosition(0, fire_pos);
            m_LineRenderer.SetPosition(1, hitInfo.point);
        } else
        {
            m_LineRenderer.SetPosition(0, fire_pos);
            m_LineRenderer.SetPosition(1, fire_pos + (Vector2)transform.up * -100);
        }
        
    }
}
