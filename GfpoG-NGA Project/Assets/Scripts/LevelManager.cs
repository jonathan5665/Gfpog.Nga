using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.Events;

// Manages The Levels
public class LevelManager : MonoBehaviour
{

    // settings
    [SerializeField] private int m_TotalLives;              // the amount of deaths before a reset                          
    [SerializeField] private GameObject m_SpawnPoint;       // a GameObject positioned where the player will spawn
    [SerializeField] private GameObject m_SpawnZone;        // The zone in which a character can be changed
    [SerializeField] GameObject[] m_CharacterPrefabs;       // The Charcters available in the game

    // Events
    public UnityEvent m_OnSpawnEvent;                       // Gets called when the player is respawned      
    public UnityEvent m_OnRagdollEvent;                     // When ragdoll state is enabled
    public UnityEvent m_OnRespawnEvent;                     // Player Respawn animation is going

    // the current state of the level / gameplay in the level
    public enum LevelState { Playing, Ragdoll, RespawnAnimation, PauseMenu };
    public LevelState m_LevelState = LevelState.Playing;

    [HideInInspector] public GameObject m_Corpses;          // the GameObject the corpses will be childed to

    // variables
    private GameManager m_GameManager;                      // the game manager
    private GameObject m_NextCharacter;                     // the character that will be spawned
    private CharacterController2D m_Player;                 // the player
    private int m_CurrentLives;                             // the amount of deaths still left
    private bool m_CanChangeCharacter = true;               // true if the player is allowed to change his character
    private bool m_HasFired = false;                        // used to keep the dropdown from firing twice
    private PauseMenu m_PauseMenu;                          // The pause menu
    private Dropdown m_DudeSelectDropdown;                  // the dropdown menu that selects the next spawn
    private CameraMovement m_Cam;                           // the camera that will be attatched to the player


    private void Awake()
    {
        m_OnSpawnEvent = new UnityEvent();
        m_OnRagdollEvent = new UnityEvent();
        m_OnRespawnEvent = new UnityEvent();
    }

    // Start is called before the first frame update
    void Start()
    {
        // get a reference to the game manager
        m_GameManager = GameManager.s_instance;

        // get a reference to the camera so that a player can be added
        GameObject cam = GameObject.Find("Main Camera");
        m_Cam = cam.GetComponent<CameraMovement>();

        // get all values needed
        m_PauseMenu = m_GameManager.m_PauseMenu.GetComponent<PauseMenu>();
        m_DudeSelectDropdown = m_PauseMenu.GetComponentInChildren<Dropdown>();

        // add listener to dropdown
        m_DudeSelectDropdown.onValueChanged.AddListener(delegate { OnDudeSelect(m_DudeSelectDropdown); });

        // set the current character to the first character in the character prefab list
        m_NextCharacter = m_CharacterPrefabs[0];

        // prepares the scene with everything that is needed
        PrepareScene();

        SpawnPlayer();
       
        m_CurrentLives = m_TotalLives;

        // reset timescale
        Time.timeScale = 1f;

        // start ragdoll if player dies
        CharacterController2D.OnDeathEvent.AddListener(StartRagdoll);

        // subscribe to pause and unpause events
        m_PauseMenu.m_OnPaused.AddListener(OnPaused);
        m_PauseMenu.m_OnUnpaused.AddListener(OnUnpaused);
    }

    // prepares everything needed in the scene
    private void PrepareScene()
    {
        // add listener to dropdown
        m_DudeSelectDropdown.onValueChanged.AddListener(delegate
        {
            OnDudeSelect(m_DudeSelectDropdown);
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

        // fill dropdown slider with information about the characters
        List<string> DropdownOptions = new List<string> { };
        foreach (GameObject character in m_CharacterPrefabs)
        {
            DropdownOptions.Add(character.name);
        }

        m_DudeSelectDropdown.AddOptions(DropdownOptions);
    }

    // Update is called once per frame
    void Update()
    {

        // allow the dropdown to fire again
        m_HasFired = false;

        // check player inputs and the game state is a state in which it can be toggled
        if (Input.GetKeyDown("tab") && (m_LevelState == LevelState.Playing || m_LevelState == LevelState.PauseMenu))
        {
            // toggle pause menu
            m_PauseMenu.TogglePauseMenu();
        }

        // different level states
        if (m_LevelState == LevelState.Playing)
        {
            StatePlaying();
        } else if (m_LevelState == LevelState.Ragdoll)
        {
            StateRagdoll();
        } else if (m_LevelState == LevelState.RespawnAnimation)
        {
            StateRespawnAnimation();
        }
    }

    private void StatePlaying()
    {

    }

    private void StateRagdoll()
    {
        if (m_Player.HasRagdollEnded())
        {
            RagdollEnd();
            DestoryPlayer();
            m_OnRespawnEvent.Invoke();
            m_LevelState = LevelState.RespawnAnimation;
        }
    }

    private void StateRespawnAnimation()
    {
        // make respawn animation (move camera)
        if (m_Cam.IsAnimationDone())
        {
            SpawnPlayer();
            m_LevelState = LevelState.Playing;
        }
    }

    // gets called when the pause menu paused the game
    private void OnPaused()
    {
        m_LevelState = LevelState.PauseMenu;
    }

    // gets called when the pause menu unpaused the game
    private void OnUnpaused()
    {
        m_LevelState = LevelState.Playing;
    }

    private void SpawnPlayer()
    {
        // create a new Player instance and place it at the spawn point
        m_Player = Instantiate(m_NextCharacter, m_SpawnPoint.transform.position, Quaternion.identity).GetComponent<CharacterController2D>();

        // attatch the player to the Camera
        m_Cam.player = m_Player.gameObject;

        Debug.Log("spawned player");

        m_LevelState = LevelState.Playing;
        GameManager.IsInputEnabled = true;
        m_CanChangeCharacter = true;
        m_OnSpawnEvent.Invoke();
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

    // will be called at the end of the ragdoll
    private void RagdollEnd()
    {
        Debug.Log("Lives: " + m_CurrentLives);

        m_Player.OnRagdollEnd();
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

    private void StartRagdoll()
    {
        m_LevelState = LevelState.Ragdoll;
        m_Player.StartRagdoll();
        GameManager.IsInputEnabled = false;
        m_OnRagdollEvent.Invoke();
    }

    public CharacterController2D GetPlayer()
    {
        return m_Player;
    }

    // A new dude has been selected in the dropdown menu
    public void OnDudeSelect(Dropdown change)
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
    }
}
