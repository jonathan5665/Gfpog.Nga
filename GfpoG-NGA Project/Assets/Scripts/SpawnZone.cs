using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [HideInInspector] public delegate void OnSpawnLeave();
    [HideInInspector] public OnSpawnLeave m_OnSpawnLeave;

    private void OnTriggerExit2D(Collider2D collision)
    {
        // maybe we should check if the player leaves the spawn
        m_OnSpawnLeave();
    }
}
