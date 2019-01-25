using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// keyboard control for player movement
public class PlayerMovement : MonoBehaviour
{
    // settings
    [SerializeField] private float m_RunSpeed = 40f;    // The speed at which the player can move
        
    // external variables
    private CharacterController2D m_Controller;  // The physics controller for the caracter used to actually move him

    // internal variables
    float m_HorizontalMove = 0f;                // A variable to store the intention of movement
    bool m_Jump = false;                        // A variable to store the intention of jumping


    // Start is called before the first frame update
    void Start()
    {
        m_Controller = gameObject.GetComponent<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Don't do anything if Input is disabled
        m_HorizontalMove = Input.GetAxisRaw("Horizontal") * m_RunSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            m_Jump = true;
        } else if (Input.GetButtonUp("Jump"))
        {
            m_Jump = false;
        }
    }

    // use FixedUpdate for physics based code. This will always be called a fixed amount per unit time.
    private void FixedUpdate()
    {
        // Don't do anything if input is disabled
        if (!GameManager.IsInputEnabled)
        {
            return;
        }
        // move the character by a fixed amount per unit time
        m_Controller.Move(m_HorizontalMove * Time.fixedDeltaTime, false, m_Jump);
    }
}
