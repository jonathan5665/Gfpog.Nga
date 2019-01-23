using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Spikes : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var tilePos = tilemap.WorldToCell(collision.transform.position);
        Debug.Log(tilePos);
    }
}
