using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextCharacterImage : MonoBehaviour
{
    // should be added automatically
    public GameObject m_levelObject;     // to get information about the level

    private LevelManager m_level;

    // Start is called before the first frame update
    void Start()
    {
        m_level = m_levelObject.GetComponent<LevelManager>();
        if (m_level == null)
        {
            Debug.Log(gameObject.name + ": ERROR level object has no LevelController");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // maybe this should only be done on a character change event or something
        GetComponent<Image>().sprite = m_level.GetNextCharacter().GetComponent<SpriteRenderer>().sprite;
    }
}
