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
    [SerializeField] private GameObject m_SpawnZone;                        // The zone in which a character can be changed
    [SerializeField] private float m_MinRagdolVel = 1f;                     // velocity at which ragdolling ends
    [Range(30, 100)] [SerializeField] private float m_CameraPanSpeed = 100f;                   // The speed for the death animation

    [HideInInspector] public GameObject m_Corpses;                          // the GameObject the corpses will be childed to
    [HideInInspector] public Dropdown m_DudeSelectDropdown;                 // the dropdown menu that selects the next spawn
    [HideInInspector] public CameraMovement m_Cam;                          // the camera that will be attatched to the player

    // the current state of the level / gameplay in the level
    public enum LevelState { Playing, Ragdoll, RespawnAnimation };
    public LevelState m_LevelState = LevelState.Playing;

    // variables
    [HideInInspector] public GameObject[] m_CharacterPrefabs;               // the available character prefabs
    [HideInInspector] public GameManager m_GameManager;                     // the game manager
    private GameObject m_NextCharacter;                                     // the character that will be spawned
    private CharacterController2D m_Player;                                 // the player
    private int m_CurrentLives;                                             // the amount of deaths still left
    private Tilemap m_TestMap;
    private bool m_CanChangeCharacter = true;                               // true if the player is allowed to change his character
    private bool m_HasFired = false;                                        // used to keep the dropdown from firing twice
    private bool m_IsPlayerDead = false;                                    // keeps track if the player was killed this round to avoid killing him twice

    // Start is called before the first frame update
    void Start()
    {
        // set the current character to the first character in the character prefab list
        m_NextCharacter = m_CharacterPrefabs[0];

        // prepares the scene with everything that is needed
        PrepareScene();

        SpawnPlayer();
       
        m_CurrentLives = m_TotalLives;
    }

    // Update is called once per frame
    void Update()
    {
        // allow the dropdown to fire again
        m_HasFired = false;

        if (m_IsPlayerDead && HasRagdollEnded())
        {
            LeaveCorpse();
            DestoryPlayer();
            m_LevelState = LevelState.RespawnAnimation;
            m_Cam.DeathAnimation(m_CameraPanSpeed, m_SpawnPoint.transform.position);
        } else if (m_LevelState == LevelState.RespawnAnimation)
        {
            // make respawn animation (move camera)
            if (m_Cam.IsAnimationDone())
            {
                SpawnPlayer();
                m_LevelState = LevelState.Playing;
            }
        }
    }

    private bool HasRagdollEnded()
    {
        // check if ragdoll has ended
        bool slowed = m_Player.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude < m_MinRagdolVel;
        if (slowed && (m_Player.IsGrouned() || m_Player.IsTouchingSpikes))
        {
            return true;
        }
        return false;
    }

    private void SpawnPlayer()
    {
        // create a new Player instance and place it at the spawn point
        m_Player = Instantiate(m_NextCharacter, m_SpawnPoint.transform.position, Quaternion.identity).GetComponent<CharacterController2D>();

        // attatch the player to the Camera
        m_Cam.player = m_Player.gameObject;

        // attatch the player transform to all objects that parallax
        AssignPlayerToParralax();
        Debug.Log("spawned player");

        m_IsPlayerDead = false;
        GameManager.IsInputEnabled = true;
        m_CanChangeCharacter = true;
    }

    private void DestoryPlayer()
    {
        Destroy(m_Player.gameObject);
    }

    private void Respawn()
    {
        DestoryPlayer();
        SpawnPlayer();
    }

    // prepares everything needed in the scene
    private void PrepareScene()
    {
        // add listener to dropdown
        m_DudeSelectDropdown.onValueChanged.AddListener(delegate
        {
            SelectDude(m_DudeSelectDropdown);
        });

        // give spawnzone the event method
        if (m_SpawnZone == null)
        {
            m_CanChangeCharacter = false;
        }
        else
        {
            m_SpawnZone.GetComponent<SpawnZone>().m_OnSpawnLeave = delegate { OnSpawnLeave(); };
        }

        // create game object for corpses and child it to level
        m_Corpses = new GameObject("Corpses");
        m_Corpses.transform.parent = gameObject.transform;
    }

    // Kills the player
    public void KillPlayer(GameObject trap)
    {
        m_IsPlayerDead = true;
        m_LevelState = LevelState.Ragdoll;
        // m_Player.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0;
        GameManager.IsInputEnabled = false;
        return;
    }

    // will be called at the end of the ragdol
    private void LeaveCorpse()
    {
        // place a corpse
        GameObject corpse = Instantiate(m_Player.GetComponent<CharacterController2D>().m_Corpse, m_Player.gameObject.transform.position, Quaternion.identity, m_Corpses.transform);

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

    // assigns the current player to all objects that use the parallax script
    private void AssignPlayerToParralax()
    {
        Parallax[] scripts = GetComponentsInChildren<Parallax>();
        foreach (Parallax script in scripts)
        {
            script.m_Controller = m_Player.GetComponent<Transform>();
            script.CaptureStartPos();
        }
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
