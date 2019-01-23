using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This handles all collisions for any corpse on the scene
public class CorpseInterface : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // check type of corpse

        // pipe to correct function
        BasicCorpse(collision);
    }

    // the collision function for the basic corpse
    private void BasicCorpse(Collision2D collision)
    {
    }

}   
