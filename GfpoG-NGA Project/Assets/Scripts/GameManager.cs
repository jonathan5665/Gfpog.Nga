using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // settings
    [Range(0, 100)] [SerializeField] private int smoothFrames = 30;
    [SerializeField] Canvas uiCanvas;

    // variables

    // later on this should be a more general Gamestate variable
    private bool paused = false;

    // for easing the pause process
    private bool timeTrans = false;         // if the pause ease in process started
    private float goalTime;                 // the goal of the time ease process
    private float startTime;                // the time the ease in process starts with


    private int time;

    // Start is called before the first frame update
    void Start()
    {
        // disable ui
        uiCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // pausing starts
        if (Input.GetKeyDown("tab"))
        {
            if (paused)
            {
                goalTime = 1f;
                startTime = 0f;
            } else
            {
                goalTime = 0f;
                startTime = 1f;
                uiCanvas.enabled = true;
            }
            timeTrans = true;
            time = 0;
        }

        // ease in the time stop
        if (timeTrans)
        {
            EaseMenu();
        }
    }

    private void EaseMenu()
    {
        // linear easing formula to fade in the menu and fade out time.
        time += 1;
        float newTime = (goalTime - startTime) / smoothFrames * time + startTime;
        Time.timeScale = newTime;

        RectTransform[] rects = uiCanvas.GetComponentsInChildren<RectTransform>();
        foreach (RectTransform rect in rects)
        {
            rect.localScale = new Vector3(1f + 0.5f * Mathf.Sin(newTime * Mathf.PI * 0.5f), 1f - newTime, 1f);
        }


        // easing is complete and the game is stopped
        if (newTime == 1f || newTime == 0f)
        {
            timeTrans = false;
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (paused)
        {
            Debug.Log("unpaused");
            paused = false;
            // disable ui
            uiCanvas.enabled = false;
        } else
        {
            Debug.Log("paused");
            paused = true; 
        }
    }
}
