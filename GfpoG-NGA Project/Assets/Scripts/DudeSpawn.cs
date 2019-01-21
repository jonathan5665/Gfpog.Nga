using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DudeSpawn : MonoBehaviour
{
    public GameObject dude;
    public DudeController currentDude;

    // Start is called before the first frame update
    void Start()
    {
        NextDude();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextDude()
    {
        currentDude = Instantiate(dude, transform.position, Quaternion.identity).GetComponent<DudeController>(); //Spawn a new dude
        currentDude.spawn = this;
    }
}
