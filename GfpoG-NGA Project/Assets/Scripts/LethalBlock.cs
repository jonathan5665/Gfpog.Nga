using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LethalBlock : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D with)
    {
        //Check if the collision was with the player
        DudeController player = with.GetComponent<DudeController>();
        if (player != null)
        {
            player.Kill(transform);
        }
    }
}
