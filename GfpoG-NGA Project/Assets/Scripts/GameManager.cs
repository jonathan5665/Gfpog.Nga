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
    [SerializeField] Canvas m_UIPrefab;                                 // The canvas for the ui
    [SerializeField] Canvas m_PauseMenuPrefab;                          // The canvas for the pause menu
    [SerializeField] EventSystem m_EventSystemPrefab;                   // The event system for the ui

    // variables
    // later on this should be a more general Gamestate variable
    public enum Gamestate { Playing};

    public static Gamestate m_Gamestate = Gamestate.Playing;


    private GameObject m_level;                 // the level current level
    private LevelManager m_LevelManager;        // the level manager for the current level
    private Canvas m_UI;                        // The Main ui
    public Canvas m_PauseMenu;                 // The Pause Menu ui

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

    // initializes the game for each level
    private void InitGame()
    {
        m_level = GameObject.Find("Level");
        if (m_level == null)
        {
            Debug.Log(gameObject.name + ": Failed to find Level");
        }
        m_LevelManager = m_level.GetComponent<LevelManager>();

        // instantiate EventSystem for ui
        Instantiate(m_EventSystemPrefab);

        // instantiate the ui elements
        m_UI = Instantiate(m_UIPrefab, m_level.transform);
        m_PauseMenu = Instantiate(m_PauseMenuPrefab, m_level.transform);

        // give the ui the camera
        GameObject cam = GameObject.Find("Main Camera");
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

        t_dropdown.onValueChanged.AddListener(delegate { m_LevelManager.OnDudeSelect(t_dropdown); });

        Button t_button = m_PauseMenu.GetComponentInChildren<Button>();
        t_button.onClick.AddListener(delegate { this.ReloadScene(); });
        
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
