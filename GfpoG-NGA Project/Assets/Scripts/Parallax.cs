using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    // the proportion of parallaxing
    [Range(0f, 2f)] [SerializeField] private float m_ParalaxProp = 0.5f;

    
    public Transform m_Controller;          // the transform that controls the parallax
    public Vector3 m_StartPos;              // the reference position for parallaxing

    private Transform m_ParentTransform;    // the transform that will be changed

    // Start is called before the first frame update
    void Start()
    {
        m_ParentTransform = GetComponentInParent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        m_ParentTransform.position = (m_Controller.position - m_StartPos) * m_ParalaxProp;
    }

    // sets the start pos to the current pos
    public void CaptureStartPos()
    {
        m_StartPos = m_Controller.position;
    }
}
