using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Spikes : MonoBehaviour
{
    private Tilemap tilemap;
    private LevelController level;

    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        level = GetComponentInParent<LevelController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // the normal on the player in the direction of the collision
        Vector3 surfaceNormal = collision.GetContact(0).normal;
        // add the surface Normal to the player position to get the position of the tile
        Vector3Int tilePos = tilemap.WorldToCell(collision.collider.transform.position + surfaceNormal);
        level.KillPlayer(tilemap, tilePos);
    }
}
