using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// Manages The Levels
public class LevelManager : MonoBehaviour
{

    // settings
    [SerializeField] private int m_TotalLives;                              // the amount of deaths before a reset                          
    [SerializeField] private GameObject m_SpawnPoint;                       // a GameObject positioned where the player will spawn
    [SerializeField] private Tilemap m_CorpseMap;                           // the tilemap in which corpses will be places
    [SerializeField] private GameObject m_SpawnZone;                        // The zone in which a character can be changed

    [HideInInspector] public Dropdown m_DudeSelectDropdown;                 // the dropdown menu that selects the next spawn
    [HideInInspector] public CameraMovement m_Cam;                          // the camera that will be attatched to the player

    // variables
    [HideInInspector] public GameObject[] m_CharacterPrefabs;               // the available character prefabs
    [HideInInspector] public GameManager m_GameManager;                     // the game manager
    private GameObject m_NextCharacter;                                     // the character that will be spawned
    private CharacterController2D m_Player;                                 // the player
    private int m_CurrentLives;                                             // the amount of deaths still left
    private Tilemap m_TestMap;
    private bool m_CanChangeCharacter = true;                                      // true if the player is allowed to change his character
    private bool m_HasFired = false;    // used to keep the dropdown from firing twice

    // Start is called before the first frame update
    void Start()
    {
        // set the current character to the first character in the character prefab list
        m_NextCharacter = m_CharacterPrefabs[0];

        // prepares the scene with everything that is needed
        PrepareScene();

        SpawnPlayer();
       
        m_CurrentLives = m_TotalLives;

        // add listener to dropdown
        m_DudeSelectDropdown.onValueChanged.AddListener(delegate
        {
            SelectDude(m_DudeSelectDropdown);
        });

        // give spawnzone the event method
        if (m_SpawnZone == null)
        {
            m_CanChangeCharacter = false;
        } else
        {
            m_SpawnZone.GetComponent<SpawnZone>().m_OnSpawnLeave = delegate { OnSpawnLeave(); };
        }
    }

    // Update is called once per frame
    void Update()
    {
        // allow the dropdown to fire again
        m_HasFired = false;
    }

    private void SpawnPlayer()
    {
        // create a new Player instance and place it at the spawn point
        m_Player = Instantiate(m_NextCharacter, m_SpawnPoint.transform.position, Quaternion.identity).GetComponent<CharacterController2D>();

        // attatch the player to the Camera
        m_Cam.player = m_Player.gameObject;

        Debug.Log("spawned player");
    }

    private void Respawn()
    {
        Destroy(m_Player.gameObject);
        SpawnPlayer();
        m_CanChangeCharacter = true;
    }

    // prepares everything needed in the scene
    private void PrepareScene()
    {
    }

    // Kills the player
    public void KillPlayer(Tilemap map, Vector3Int tilePos)
    {
        // destroy the kill source and regenerate Composite collider
        map.SetTile(tilePos, null);
        map.GetComponent<CompositeCollider2D>().GenerateGeometry();

        // place a corpse
        m_CorpseMap.SetTile(tilePos, m_Player.corpse);
        m_CorpseMap.GetComponent<CompositeCollider2D>().GenerateGeometry();

        Debug.Log("Lives: " + m_CurrentLives);

        // respawn or reset the level
        if (m_CurrentLives > 1)
        {
            //Remove a life
            m_CurrentLives -= 1;

            //Places the player at the spawnPoint. This very likely needs to redone once we get more characters.
            Respawn();
        }
        else
        {
            Debug.Log("Reset Level");
            //Reload the current scene. This needs to be redone once we get more levels.
            m_GameManager.ReloadScene();
        }
    }

    public CharacterController2D GetPlayer()
    {
        return m_Player;
    }

    // A new dude has been selected in the dropdown menu
    public void SelectDude(Dropdown change)
    {
        // for some reason this can be fired twice. So here I check that it was the first time;
        if (m_HasFired)
        {
            return;
        }
        m_HasFired = true;

        Debug.Log("select dude");
        string option = change.captionText.text;
        GameObject choice = null;

        foreach (GameObject character in m_CharacterPrefabs)
        {
            if (character.name == change.captionText.text)
            {
                choice = character;
                break;
            }
        }

        if (choice != null)
        {
            m_NextCharacter = choice;
            if (m_CanChangeCharacter)
            {
                Respawn();
            }
        }
    }

    public GameObject GetNextCharacter()
    {
        return m_NextCharacter;
    }

    public int GetCurrLives()
    {
        return m_CurrentLives;
    }

    // called when the character leaves the spawn zone
    public void OnSpawnLeave()
    {
        m_CanChangeCharacter = false;
        Debug.Log("left spawn zone");
    }
}
