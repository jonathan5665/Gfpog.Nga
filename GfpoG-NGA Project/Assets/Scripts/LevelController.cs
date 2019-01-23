using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class LevelController : MonoBehaviour
{

    // settings
    [SerializeField] private int totalLives;            // the amount of deaths before a reset                          
    [SerializeField] private GameObject spawnPoint;     // a GameObject positioned where the player will spawn
    [SerializeField] private GameObject character;      // the pefab for the player
    [SerializeField] private CameraMovement cam;        // the camera that will be attatched to the player
    [SerializeField] private Tilemap corpseMap;         // the tilemap in which corpses will be places

    // variables
    private CharacterController2D player;               // the player
    private int currentLives;                           // the amount of deaths still left

    // Start is called before the first frame update
    void Start()
    {
        // create a new Player instance and place it at the spawn point
        player = Instantiate(character, spawnPoint.transform.position, Quaternion.identity).GetComponent<CharacterController2D>();
        // attatch the player to the Camera
        cam.player = player.gameObject;
        currentLives = totalLives;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Kills the player
    public void KillPlayer(Tilemap map, Vector3Int tilePos)
    {
        // destroy the kill source and regenerate Composite collider
        map.SetTile(tilePos, null);
        map.GetComponent<CompositeCollider2D>().GenerateGeometry();

        // place a corpse
        corpseMap.SetTile(tilePos, player.corpse);

        Debug.Log("Lives: " + currentLives);

        // respawn or reset the level
        if (currentLives > 0)
        {
            //Remove a life
            currentLives -= 1;

            //Places the player at the spawnPoint. This very likely needs to redone once we get more characters.
            player.transform.position = spawnPoint.transform.position;
        }
        else
        {
            Debug.Log("Reset Level");
            //Reload the current scene. This needs to be redone once we get more levels.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
