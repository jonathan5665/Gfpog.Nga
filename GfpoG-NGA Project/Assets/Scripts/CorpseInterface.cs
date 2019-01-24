using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// This handles all collisions for any corpse on the scene
public class CorpseInterface : MonoBehaviour
{
    // Variables
    [Header("Fat Corpse")]
    [Range(0, 1500)]  [SerializeField] private float m_FatCorpseForce = 1000f;      // the jump force the fat corpse applies

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collision");
        // get the player
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        if (player == null)
        {
            return;  // only do something if colliding with a player
        }

        // the normal on the player in the direction of the collision
        Vector3 surfaceNormal = collision.GetContact(0).normal;
        // add the surface Normal to the player position to get the position of the tile. This will only give the tile directly besides / under / over the player.
        Vector3Int tilePos = tilemap.WorldToCell(collision.collider.transform.position + surfaceNormal);

        TileBase tile = tilemap.GetTile(tilePos);

        // if the tile that collides with the player does only collide on a corner the tile will be null
        if (tile == null)
        {
            Debug.Log("ERROR no collision found");
            return;
        }

        // check type of corpse and pipe to correct function
        // this should be done via some kind of datastructure
        if (tile.name == "DudeCorpse")
        {
            BasicCorpse(collision);
        } else if (tile.name == "FatCorpse")
        {
            FatCorpse(collision);
        }
    }

    // the collision function for the basic corpse
    private void BasicCorpse(Collision2D collision)
    {
    }

    private void FatCorpse(Collision2D collision)
    {
        CharacterController2D player = collision.gameObject.GetComponent<CharacterController2D>();
        if (player == null)
        {
            return;
        }
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
        Debug.Log(collision.gameObject.name);
        rb.AddForce(new Vector2(0f, m_FatCorpseForce));
        // no extra jumping on these
        player.DenyJump();
    }
}   
