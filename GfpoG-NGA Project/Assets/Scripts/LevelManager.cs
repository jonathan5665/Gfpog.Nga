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
    [SerializeField] private int m_TotalLives;                                  // the amount of deaths before a reset                          
    [SerializeField] private GameObject m_SpawnPoint;                           // a GameObject positioned where the player will spawn
    [SerializeField] private GameObject m_SpawnZone;                            // The zone in which a character can be changed
    [Range(0, 0.5f)] [SerializeField] private float m_PauseMenuEaseTime = 0.1f; // the time for the start menu to ease in
    [SerializeField] GameObject[] m_CharacterPrefabs;                           // The Charcters available in the game
    [Range(0, 2)] [SerializeField] private float m_PauseMenuSlide = 1f;         // The amount the pause menu slides up when paused

    // The settings for the pause menu ease
    private struct PauseMenuEase
    {
        public bool IsFinished;             // true if ease is finished
        public float StartTime;             // the time the ease starts
        public float Duration;              // the time the ease takes
        public bool IsEasingIn;             // a variable to check if we are easing in our out of the pause menu
        public float StartValue;

        public PauseMenuEase(bool isFinished, float startTime, float easeTime, bool isEasingIn, float startValue)
        {
            IsFinished = isFinished;
            StartTime = startTime;
            Duration = easeTime;
            IsEasingIn = isEasingIn;
            StartValue = startValue;
        }
    }

    private PauseMenuEase m_PauseMenuEase;

    [HideInInspector] public GameObject m_Corpses;                              // the GameObject the corpses will be childed to
    [HideInInspector] public Dropdown m_DudeSelectDropdown;                     // the dropdown menu that selects the next spawn
    [HideInInspector] public CameraMovement m_Cam;                              // the camera that will be attatched to the player

    // private settings
    private Canvas m_PauseMenu;                                                 // The canvas for the pause menu

    public UnityEvent m_OnSpawnEvent;                                         // Gets called when the player is respawned      
    public UnityEvent m_OnRagdollEvent;                                       // When ragdoll state is enabled
    public UnityEvent m_OnRespawnEvent;                                       // Player Respawn animation is going

    // the current state of the level / gameplay in the level
    public enum LevelState { Playing, Ragdoll, RespawnAnimation, PauseMenu };
    public LevelState m_LevelState = LevelState.Playing;

    // variables
    [HideInInspector] public GameManager m_GameManager;                     // the game manager
    private GameObject m_NextCharacter;                                     // the character that will be spawned
    private CharacterController2D m_Player;                                 // the player
    private int m_CurrentLives;                                             // the amount of deaths still left
    private Tilemap m_TestMap;
    private bool m_CanChangeCharacter = true;                               // true if the player is allowed to change his character
    private bool m_HasFired = false;                                        // used to keep the dropdown from firing twice

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
        Debug.Log(GameObject.Find("GameManager"));
        m_GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        // get a reference to the camera so that a player can be added
        GameObject cam = GameObject.Find("Main Camera");
        m_Cam = cam.GetComponent<CameraMovement>();

        // get all values needed
        m_PauseMenu = m_GameManager.m_PauseMenu;

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
    }

    // prepares everything needed in the scene
    private void PrepareScene()
    {
        // disable pause menu ui
        m_PauseMenu.enabled = false;

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

        // check player inputs
        if (Input.GetKeyDown("tab"))
        {
            // only allow pause menu if playing
            if (m_LevelState == LevelState.Playing)
            {
                m_LevelState = LevelState.PauseMenu;
                m_PauseMenu.enabled = true;
                m_PauseMenuEase = new PauseMenuEase(false, Time.realtimeSinceStartup, m_PauseMenuEaseTime, true, Time.timeScale);
            } else if (m_LevelState == LevelState.PauseMenu)
            {
                // reverse direction of ease
                m_PauseMenuEase.IsEasingIn = false;
                m_PauseMenuEase.IsFinished = false;
                m_PauseMenuEase.StartTime = Time.realtimeSinceStartup;
                m_PauseMenuEase.StartValue = Time.timeScale;
            }
        }

        // different level states
        if (m_LevelState == LevelState.PauseMenu)
        {
            StatePauseMenu();
        } else if (m_LevelState == LevelState.Playing)
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

    private void StatePauseMenu()
    {
        if (!m_PauseMenuEase.IsFinished)
        {
            // Ease into pause Menu
            EasePauseMenu();
        }
        else if (m_PauseMenuEase.IsEasingIn)
        {
            // easing in and done
        }
        else
        {
            // Easing out and done. Pause menu has endet
            m_LevelState = LevelState.Playing;
            // close pause menu
            m_PauseMenu.enabled = false;
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

    private void EasePauseMenu()
    {
        float currentTime = Time.realtimeSinceStartup - m_PauseMenuEase.StartTime;
        float valueChange;

        if (m_PauseMenuEase.IsEasingIn)
        {
            valueChange = -m_PauseMenuEase.StartValue;
        } else
        {
            valueChange = 1f - m_PauseMenuEase.StartValue;
        }

        float value;

        // check if done with ease
        if (currentTime > m_PauseMenuEase.Duration)
        {
            // done with ease
            value = m_PauseMenuEase.StartValue + valueChange;
            m_PauseMenuEase.IsFinished = true;
        } else
        {
            value = UtilityScript.LinearTween(currentTime, m_PauseMenuEase.StartValue, valueChange, m_PauseMenuEase.Duration);
        }

        // set time to value
        Time.timeScale = value;

        if (value == m_PauseMenuEase.StartValue + valueChange)
        {
            m_PauseMenuEase.IsFinished = true;
        }
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
