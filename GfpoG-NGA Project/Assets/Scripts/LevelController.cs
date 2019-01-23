using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{

    // settings
    [SerializeField] private int totalLives;            // the amount of deaths before a reset                          
    [SerializeField] private GameObject spawnPoint;     // a GameObject positioned where the player will spawn
    [SerializeField] private GameObject dudeCharacter;      // the pefab for the player
    [SerializeField] private GameObject fatCharacter;      // the pefab for the player
    [SerializeField] private CameraMovement cam;        // the camera that will be attatched to the player
    [SerializeField] private Tilemap corpseMap;         // the tilemap in which corpses will be places

    [SerializeField] private Dropdown dudeSelect;       // the dropdown menu that selects the next spawn

    [SerializeField] private GameObject currCharImage;  // the panel in which the current character is displayed
    [SerializeField] private GameObject textField;      // the text field in which to write the lives

    private GameObject character;

    // variables
    private CharacterController2D player;               // the player
    private int currentLives;                           // the amount of deaths still left

    // Start is called before the first frame update
    void Start()
    {
        character = dudeCharacter;
        SetCharacterImage(character.GetComponent<SpriteRenderer>().sprite);

        // create a new Player instance and place it at the spawn point
        player = Instantiate(character, spawnPoint.transform.position, Quaternion.identity).GetComponent<CharacterController2D>();
        // attatch the player to the Camera
        cam.player = player.gameObject;
        currentLives = totalLives;

        // add listener to dropdown
        dudeSelect.onValueChanged.AddListener(delegate
        {
            SelectDude(dudeSelect);
        });

        // write the current lives to the ui
        WriteLivesToUi();
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
        corpseMap.GetComponent<CompositeCollider2D>().GenerateGeometry();

        Debug.Log("Lives: " + currentLives);

        // respawn or reset the level
        if (currentLives > 1)
        {
            //Remove a life
            currentLives -= 1;

            WriteLivesToUi();

            //Places the player at the spawnPoint. This very likely needs to redone once we get more characters.
            // player.transform.position = spawnPoint.transform.position;

            // switch character
            // create a new Player instance and place it at the spawn point
            Destroy(player.gameObject);
            player = Instantiate(character, spawnPoint.transform.position, Quaternion.identity).GetComponent<CharacterController2D>();
            // attatch the player to the Camera
            cam.player = player.gameObject;
        }
        else
        {
            Debug.Log("Reset Level");
            //Reload the current scene. This needs to be redone once we get more levels.
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public CharacterController2D GetPlayer()
    {
        return player;
    }

    // A new dude has been selected in the dropdown menu
    public void SelectDude(Dropdown change)
    {
        string option = change.captionText.text;
        if (option == "Dude")
        {
            character = dudeCharacter;
            SetCharacterImage(dudeCharacter.GetComponent<SpriteRenderer>().sprite);
        }
        else if (option == "Fat Dude")
        {
            character = fatCharacter;
            SetCharacterImage(fatCharacter.GetComponent<SpriteRenderer>().sprite);
        }
    }

    private void SetCharacterImage(Sprite sprite)
    {
        currCharImage.GetComponent<Image>().overrideSprite = sprite;
    }

    private void WriteLivesToUi()
    {
        textField.GetComponent<Text>().text = "Lives:\n" + currentLives;
    }
}
