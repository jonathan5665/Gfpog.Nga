using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // settings
    public float runSpeed = 40f;
        
    // variables
    public CharacterController2D controller;

    float horizontalMove = 0f;
    bool jump = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        } else if (Input.GetButtonUp("Jump"))
        {
            jump = false;
        }
    }

    // use FixedUpdate for physics based code. This will always be called a fixed amount per unit time.
    private void FixedUpdate()
    {
        // move the character by a fixed amount per unit time
        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
    }
}
