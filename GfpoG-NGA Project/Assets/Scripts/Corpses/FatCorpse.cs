using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FatCorpse : MonoBehaviour
{
    [Range(0, 1500)] [SerializeField] private float m_FatCorpseForce = 1000f;      // the jump force the fat corpse applies
    [Range(0, 1000)] [SerializeField] private int m_TimeoutMS = 100;                // the timeout before the collision can fire again in ms

    private float m_NextTime = 0;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check if in timeout or can fire again
        if (m_NextTime > Time.time)
        {
            return;
        }

        // check if collision with player
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        if (player == null)
        {
            return;
        }

        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

        // the normal on this object in the direction of the collision. We can't get the normal of the collision because that would be the players normal.
        ContactPoint2D[] contacts = new ContactPoint2D[collision.contactCount];
        collision.otherCollider.GetContacts(contacts);
        Vector3 surfaceNormal = contacts[0].normal;
        Debug.Log("Collision Normal " + surfaceNormal);

        rb.AddForce(surfaceNormal * -1 * m_FatCorpseForce);
        // no extra jumping on these
        player.DenyJump();

        // set time for next activation
        m_NextTime = Time.time + m_TimeoutMS / 1000;
    }
}
