using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextGetLives : MonoBehaviour
{

    // this should be assigned automatically
    public GameObject m_LevelObject;
    private LevelManager m_level;

    // Start is called before the first frame update
    void Start()
    {
        m_level = m_LevelObject.GetComponent<LevelManager>();
        if (m_level == null)
        {
            Debug.Log(gameObject.name + ": ERROR level object has no LevelController");
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Text>().text = "Lives:\n" + m_level.GetCurrLives();
    }
}
