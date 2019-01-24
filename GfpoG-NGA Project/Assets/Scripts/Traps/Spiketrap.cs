using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiketrap : MonoBehaviour
{
    private LevelManager level;

    // Start is called before the first frame update
    void Start()
    {
        level = GetComponentInParent<LevelManager>();
    }

    // OnCollisionStay instead of OnCollisionEnter because OnCollisionStay does not get triggered if a new Tile is collided with if already collided with another tile.
    // Also This will not trigger on the first frame of entering a collision which gives the character time to get grounded.
    private void OnCollisionStay2D(Collision2D collision)
    {
        // get the player
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        if (player == null)
        {
            return;  // only do something if colliding with a player
        }

        // the normal on the player in the direction of the collision
        Vector3 surfaceNormal = collision.GetContact(0).normal;

        // don't do anything if player on top and grounded
        if (surfaceNormal.y == -1 && player.IsGrouned())
        {
            return;
        }

        Debug.Log(level);
        Debug.Log(gameObject);

        level.KillPlayer(gameObject);
    }
}
