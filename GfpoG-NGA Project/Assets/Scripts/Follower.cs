using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public GameObject parent;
    private Transform m_ParentTransform;

    // Start is called before the first frame update
    void Start()
    {
        m_ParentTransform = parent.GetComponent<Transform>();
    }

    private void Update()
    {
        transform.position = m_ParentTransform.position;
    }
}
