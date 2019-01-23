using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{
    [SerializeField] private int totalLives;
    private int currentLives;

    [SerializeField] private Vector2 spawnPoint;
    [SerializeField] private GameObject character;
    [SerializeField] private CameraMovement cam;
    private CharacterController2D player;

    // Start is called before the first frame update
    void Start()
    {
        player = Instantiate(character, spawnPoint, Quaternion.identity).GetComponent<CharacterController2D>();
        cam.player = player.gameObject;
        player.gameObject.AddComponent<CharacterDude>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Kills the player
    public void KillPlayer(Tilemap map, Vector2 pos)
    {
        //Replace the tile
        Vector3Int tilePos = map.WorldToCell(pos);
        map.SetTile(tilePos, player.corpse);

        if(currentLives > 0)
        {
            //Remove a life
            currentLives -= 1;

            //Places the player at the spawnPoint. This very likely needs to redone once we get more characters.
            player.transform.position = spawnPoint;
        }
        else
        {
            //Reload the current scene. This needs to be redone once we get more levels.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
