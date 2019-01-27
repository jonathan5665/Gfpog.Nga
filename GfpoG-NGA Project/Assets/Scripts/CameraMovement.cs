using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;

    enum CameraState { Normal, Ragdoll, DeathAnimation}

    private CameraState m_CameraState = CameraState.Normal;

    [Range (0, 2)] [SerializeField] private float m_DeathAnimTime = 1f;
    [Range(0, 5)] [SerializeField] private float m_RagdollZoom = 2f;
    [Range(0, 5)] [SerializeField] private float m_RagdollZoomTime = 5f;


    private UtilityScript.EaseValues m_EaseValuesZoom;  // holds the ease values for the zoom animation
    private UtilityScript.EaseValues m_EaseValuesTrans; // holds the ease values for the translation animation

    private Vector2 m_RespawnAnimStartValue;
    private Vector2 m_SpawnPoint;
    private Vector2 m_RespawnAnimValueChange;

    private float m_RagdollAnimValueChange;

    // Start is called before the first frame update
    void Start()
    {
        // add listener for ragdoll animation
        LevelManager level = GameObject.Find("Level").GetComponent<LevelManager>();
        level.m_OnRagdollEvent.AddListener(InitRagdollAnimation);
        level.m_OnRespawnEvent.AddListener(InitRespawnAnimation);
        m_SpawnPoint = GameObject.Find("SpawnPoint").transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_CameraState == CameraState.DeathAnimation)
        {
            if (!m_EaseValuesZoom.IsFinished)
                RespawnAnimation();
        }
        else if (m_CameraState == CameraState.Ragdoll)
        {
            if (!m_EaseValuesZoom.IsFinished)
            {
                RagdollAnimation();
            } else
            {
                // ragdoll animation is finished
                TransformToPlayer();
            }
        }
        else if (m_CameraState == CameraState.Normal)
        {
            TransformToPlayer();
        }
    }

    private void TransformToPlayer()
    {
        transform.position = player.transform.position + Vector3.back * 10;
    }

    private void RespawnAnimation()
    {
        // do translation animation
        float value = UtilityScript.QuadEaseInOut(m_EaseValuesTrans);
        transform.position = (Vector3)(m_RespawnAnimStartValue + value * m_RespawnAnimValueChange) + Vector3.back * 10;

        // do zooming animation
        value = UtilityScript.QuadEaseInOut(m_EaseValuesZoom);
        gameObject.GetComponent<Camera>().orthographicSize = value;

        m_EaseValuesZoom.IsFinished = UtilityScript.IsEaseDone(m_EaseValuesZoom);
        if (m_EaseValuesZoom.IsFinished)
        {
            m_CameraState = CameraState.Normal;

            // enable pixel perfect camera
            gameObject.GetComponent<PixelPerfectCamera>().enabled = true;
        }
    }

    private void RagdollAnimation()
    {
        float value = UtilityScript.QuadEaseInOut(m_EaseValuesZoom);
        gameObject.GetComponent<Camera>().orthographicSize = value;
        TransformToPlayer();
        m_EaseValuesZoom.IsFinished = UtilityScript.IsEaseDone(m_EaseValuesZoom);
    }

    private void InitRespawnAnimation()
    {
        float currSize = gameObject.GetComponent<Camera>().orthographicSize;
        m_EaseValuesZoom = new UtilityScript.EaseValues(Time.timeSinceLevelLoad, m_DeathAnimTime, currSize, m_EaseValuesZoom.StartValue - currSize);
        m_EaseValuesTrans = new UtilityScript.EaseValues(Time.timeSinceLevelLoad, m_DeathAnimTime, 0f, 1f);
        m_RespawnAnimStartValue = (Vector2)transform.position;
        m_RespawnAnimValueChange = m_SpawnPoint - (Vector2)transform.position;
        m_CameraState = CameraState.DeathAnimation;
    }

    private void InitRagdollAnimation()
    {
        float currSize = gameObject.GetComponent<Camera>().orthographicSize;
        m_EaseValuesZoom = new UtilityScript.EaseValues(Time.timeSinceLevelLoad, m_RagdollZoomTime, currSize, m_RagdollZoom - currSize);

        // disable pixel perfect camera
        gameObject.GetComponent<PixelPerfectCamera>().enabled = false;
        
        m_CameraState = CameraState.Ragdoll;
    }

    public bool IsAnimationDone()
    {
        return m_EaseValuesZoom.IsFinished;
    }
}
