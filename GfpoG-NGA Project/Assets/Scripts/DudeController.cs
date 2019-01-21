using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DudeController : MonoBehaviour
{
    public float jumpForce;
    public float moveSpeed;

    Rigidbody2D rb;

    public GameObject corpse;
    public DudeSpawn spawn;

    bool grounded; //Necessary to see if you can jump.

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        grounded = false;

        spawn.currentDude = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Update grounded status to false.
        grounded = false;

        //Get an array of things right below the character
        Collider2D[] thingsAtFeet = Physics2D.OverlapAreaAll(transform.position + new Vector3(-.49f, 0), transform.position + new Vector3(.49f, -.51f));
        for(int i = 0; i < thingsAtFeet.Length; i++)
        {
            //Set grounded to true if there's a wall, and break the loop.
            if(thingsAtFeet[i].tag == "Wall")
            {
                grounded = true;
                break;
            }
        }

        Debug.Log(grounded);

        //Check for input

        //To the right -->
        if (Input.GetAxisRaw("horizontal") > 0)
        {
            rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        }

        //To the left <--
        else if(Input.GetAxisRaw("horizontal") < 0)
        {
            rb.velocity = new Vector2(-moveSpeed, rb.velocity.y);
        }

        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        //Jump /^\
        if(Input.GetAxisRaw("jump") > 0)
        {
            //Only jump if you're not already going up and are at the ground
            if (grounded && rb.velocity.y <= 0)
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }

    //Death
    public void Kill(Transform what)
    {
        Vector2 pos = what.transform.position;
        Destroy(what.gameObject); //Destroy the death source
        Instantiate(corpse, pos, Quaternion.identity); //Place a corpse at the location of death
        spawn.NextDude();
        Destroy(gameObject);
    }
}
