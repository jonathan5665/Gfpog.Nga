using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoothingTrap : MonoBehaviour
{
    [SerializeField] private GameObject m_ProjectilePrefab;
    [Range(0, 5000)] [SerializeField] private int m_ShootDelayMS = 1000;
    [Range(0, 10)] [SerializeField] private float m_ProjectileSpeed = 10f;  // the speed of the projectile

    private float m_NextShootTime = 0;
    private LevelManager m_Level;

    // Start is called before the first frame update
    private void Awake()
    {
        m_Level = GameObject.Find("Level").GetComponent<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (CanShoot()) Shoot();
    }

    private bool CanShoot()
    {
        // this should also check if the trap is on screen to avoid too many objects being fired
        return m_NextShootTime < Time.realtimeSinceStartup;
    }

    private void Shoot()
    {
        GameObject t_Projectile = Instantiate(m_ProjectilePrefab, transform.position, Quaternion.identity, transform);
        Projectile t_ProjectileController = t_Projectile.GetComponent<Projectile>();
        t_ProjectileController.Whitelist(gameObject);
        t_ProjectileController.SetVelocity(transform.up * m_ProjectileSpeed * -1);
        t_ProjectileController.SetLevel(m_Level);

        // set the next time the trap can shoot
        m_NextShootTime = Time.realtimeSinceStartup + m_ShootDelayMS / 1000;
    }
}
