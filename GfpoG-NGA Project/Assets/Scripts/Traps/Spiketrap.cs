using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiketrap : MonoBehaviour
{
    // settings
    [Range(0, 50)] [SerializeField] private float m_Friction = 10f;        // the magnitude of tangential friction;

    private LevelManager m_Level;       // the level so that it can be told to kill the player
    private Rigidbody2D m_PlayerRb;     // the ridgid body of the player

    // Start is called before the first frame update
    void Start()
    {
        m_Level = GetComponentInParent<LevelManager>();
    }

    private void FixedUpdate()
    {
        if (m_PlayerRb != null)
        {
            SlowPlayer();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // get the player
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        if (player == null)
        {
            return;  // only do something if colliding with a player
        }

        player.IsTouchingSpikes = true;

        m_PlayerRb = collision.gameObject.GetComponent<Rigidbody2D>();
        m_Level.KillPlayer(gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // get the player
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        if (player == null)
        {
            return;  // only do something if colliding with a player
        }
        player.IsTouchingSpikes = false;

        m_PlayerRb = null;

    }

    // slow the player in some way
    private void SlowPlayer()
    {
        m_PlayerRb.AddForce(m_PlayerRb.velocity.normalized * -1 * m_Friction);
    }
}
