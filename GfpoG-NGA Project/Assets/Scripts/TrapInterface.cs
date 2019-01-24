using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TrapInterface : MonoBehaviour
{
    private Tilemap m_Tilemap;
    private LevelManager level;

    // Start is called before the first frame update
    void Start()
    {
        m_Tilemap = gameObject.GetComponent<Tilemap>();
        level = GetComponentInParent<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
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
        // add the surface Normal to the player position to get the position of the tile. This will only give the tile directly besides / under / over the player.
        Vector3Int tilePos = m_Tilemap.WorldToCell(collision.collider.transform.position + surfaceNormal);

        TileBase tile = m_Tilemap.GetTile(tilePos);

        // if the tile that collides with the player does only collide on a corner the tile will be null
        if (tile == null)
        {
            Debug.Log("ERROR no collision found");
            return;
        }

        // check type of trap and pipe to correct function
        // this should be done via some kind of datastructure
        if (tile.name == "Spikes")
        {
            Spiketrap(collision);
        }
    }

    private void Spiketrap(Collision2D collision)
    {
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        Vector3 surfaceNormal = collision.GetContact(0).normal;

        // don't do anything if player on top and grounded
        if (surfaceNormal.y == -1 && player.IsGrouned())
        {
            return;
        }
        Vector3Int tilePos = m_Tilemap.WorldToCell(collision.collider.transform.position + surfaceNormal);
        level.KillPlayer(m_Tilemap, tilePos);
    }
}
