using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Spikes : MonoBehaviour
{
    private Tilemap tilemap;
    private LevelManager level;

    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
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
        // check if the player is grounded
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        if (player == null || player.IsGrouned())
        {
            return;
        }

        // the normal on the player in the direction of the collision
        Vector3 surfaceNormal = collision.GetContact(0).normal;
        // add the surface Normal to the player position to get the position of the tile
        Vector3Int tilePos = tilemap.WorldToCell(collision.collider.transform.position + surfaceNormal);
        level.KillPlayer(tilemap, tilePos);
    }
}
