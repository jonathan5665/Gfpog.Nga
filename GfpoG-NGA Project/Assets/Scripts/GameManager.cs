using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager s_instance = null;      //Static instance of GameManager which allows it to be accessed by any other script.

    // settings
    [Range(0, 100)] [SerializeField] private int m_SmoothFrames = 30;
    [SerializeField] Canvas m_UIPrefab;             // The canvas for the ui
    [SerializeField] Canvas m_PauseMenuPrefab;      // The canvas for the pause menu
    [SerializeField] GameObject[] m_CharacterPrefabs;
    [SerializeField] EventSystem m_EventSystemPrefab;     // The event system for the ui 

    private const string c_CharacterFolderLoad = "Assets";

    // variables
    // later on this should be a more general Gamestate variable
    public enum Gamestate { Playing, Paused};

    public static Gamestate m_Gamestate = Gamestate.Playing;


    // for easing the pause process
    private bool m_TimeTrans = false;           // if the pause ease in process started
    private float m_GoalTime;                   // the goal of the time ease process
    private float m_StartTime;                  // the time the ease in process starts with
    private int m_DurrLevel = 1;                // the current level
    private GameObject m_level;                 // the level current level
    private LevelManager m_LevelManager;        // the level manager for the current level
    private int t_time;
    private Canvas m_UI;
    private Canvas m_PauseMenu;

    // settings for the game
    public static bool IsInputEnabled = true;   // if player input is enabled

    private void Awake()
    {
        //Check if instance already exists
        if (s_instance == null)

            //if not, set instance to this
            s_instance = this;

        //If instance already exists and it's not this:
        else if (s_instance != this)

            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        InitGame();

    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // pausing starts
        if (Input.GetKeyDown("tab"))
        {
            if (m_Gamestate == Gamestate.Paused)
            {
                m_GoalTime = 1f;
                m_StartTime = 0f;
            } else
            {
                m_GoalTime = 0f;
                m_StartTime = 1f;
                m_PauseMenu.enabled = true;
            }
            m_TimeTrans = true;
            t_time = 0;
        }

        // ease in the time stop
        if (m_TimeTrans)
        {
            EaseMenu();
        }
    }

    // initializes the game for each level
    private void InitGame()
    {
        m_level = GameObject.Find("Level");
        if (m_level == null)
        {
            Debug.Log(gameObject.name + ": Failed to find Level");
        }
        m_LevelManager = m_level.GetComponent<LevelManager>();

        // give the level manager a reference to this
        m_LevelManager.m_GameManager = this;
        // give the level the characters
        m_LevelManager.m_CharacterPrefabs = m_CharacterPrefabs;
        GameObject cam = GameObject.Find("Main Camera");

        // give a main camera reference to the level controller
        m_LevelManager.m_Cam = cam.GetComponent<CameraMovement>();

        // instantiate EventSystem for ui
        Instantiate(m_EventSystemPrefab);

        // instantiate the ui elements
        m_UI = Instantiate(m_UIPrefab, m_level.transform);
        m_PauseMenu = Instantiate(m_PauseMenuPrefab, m_level.transform);

        // disable ui
        m_PauseMenu.enabled = false;


        Camera camera = cam.GetComponent<Camera>();
        m_UI.worldCamera = camera;
        m_PauseMenu.worldCamera = camera;

        // add references to the ui
        TextGetLives[] t_texts = m_UI.GetComponentsInChildren<TextGetLives>();
        foreach (TextGetLives text in t_texts)
        {
            text.m_LevelObject = m_level;
        }
        NextCharacterImage t_img = m_UI.GetComponentInChildren<NextCharacterImage>();
        t_img.m_levelObject = m_level;

        Dropdown t_dropdown = m_PauseMenu.GetComponentInChildren<Dropdown>();
        m_LevelManager.m_DudeSelectDropdown = t_dropdown;

        t_dropdown.onValueChanged.AddListener(delegate { m_LevelManager.SelectDude(t_dropdown); });

        Button t_button = m_PauseMenu.GetComponentInChildren<Button>();
        t_button.onClick.AddListener(delegate { this.ReloadScene(); });
        
    }

    private void EaseMenu()
    {
        // linear easing formula to fade in the menu and fade out time.
        t_time += 1;
        float newTime = (m_GoalTime - m_StartTime) / m_SmoothFrames * t_time + m_StartTime;
        Time.timeScale = newTime;

        RectTransform[] rects = m_PauseMenu.GetComponentsInChildren<RectTransform>();
        foreach (RectTransform rect in rects)
        {
            rect.localScale = new Vector3(1f + 0.5f * Mathf.Sin(newTime * Mathf.PI * 0.5f), 1f - newTime, 1f);
        }


        // easing is complete and the game is stopped
        if (newTime == 1f || newTime == 0f)
        {
            m_TimeTrans = false;
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (m_Gamestate == Gamestate.Paused)
        {
            Debug.Log("unpaused");
            m_Gamestate = Gamestate.Playing;
            // disable ui
            m_PauseMenu.enabled = false;
        } else
        {
            Debug.Log("paused");
            m_Gamestate = Gamestate.Paused;
        }
    }

    // inits the game after scene was loaded
    private void OnLevelWasLoaded(int level)
    {
        InitGame();
    }

    // reloads the scene
    public void ReloadScene()
    {
        // unpause if paused
        Time.timeScale = 1f;
        m_Gamestate = Gamestate.Playing;

        // reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
