using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerLoader : MonoBehaviour
{
    public GameObject gameManager;          //GameManager prefab to instantiate.

    void Awake()
    {
        //Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
        if (GameManager.s_instance == null)

            //Instantiate gameManager prefab
            Instantiate(gameManager);
    }
}

