using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PauseMenu : MonoBehaviour
{
    // settings
    [Range(0, 0.5f)] [SerializeField] private float m_PauseMenuEaseTime = 0.3f; // the time for the start menu to ease in

    public UnityEvent m_OnPaused;
    public UnityEvent m_OnUnpaused;

    // variables
    private bool m_IsPaused;
    private Canvas m_Canvas;

    // the ease values for the pause script
    private UtilityScript.EaseValues m_EaseValues;

    private void Awake()
    {
        m_OnPaused = new UnityEvent();
        m_OnUnpaused = new UnityEvent();

        m_Canvas = GetComponent<Canvas>();
        m_Canvas.enabled = false;
        m_IsPaused = false;

        // set ease values to be finished so that there is no interpolation
        m_EaseValues = new UtilityScript.EaseValues();
        m_EaseValues.IsFinished = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_EaseValues.IsFinished)
        {
            if (UtilityScript.IsEaseDone(m_EaseValues, true))
            {
                if (m_IsPaused)
                {
                    // unpaused
                    m_OnUnpaused.Invoke();
                    Time.timeScale = 1f;
                    m_Canvas.enabled = false;
                }
                else
                {
                    // paused
                    m_OnPaused.Invoke();
                    Time.timeScale = 0f;
                
                }
                m_IsPaused = !m_IsPaused;
                m_EaseValues.IsFinished = true;
            } else
            {
                Time.timeScale = UtilityScript.QuadEaseInOut(m_EaseValues, true);
            }
        }
    }

    public void TogglePauseMenu()
    {
        if (m_IsPaused)
        {
            // start unpausing
            m_EaseValues = new UtilityScript.EaseValues(Time.realtimeSinceStartup, m_PauseMenuEaseTime, Time.timeScale, 1f - Time.timeScale);

        }
        else
        {
            // start pausing
            m_EaseValues = new UtilityScript.EaseValues(Time.realtimeSinceStartup, m_PauseMenuEaseTime, Time.timeScale, -Time.timeScale);
            // enable canvas for animation
            m_Canvas.enabled = true;
        }
    }
}
