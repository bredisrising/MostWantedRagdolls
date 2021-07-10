using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    public Rigidbody upperArm;
    public AutoAim autoAim;
    public EnemyAutoAim enemyAutoAim;

    public float recoil;
    public float hitForce;
    public float jumpForce;

    public float bloom;

    public float fireRate;

    Transform target;

    public GameObject projectile;
    public float projectileSpeed;
    public float spawnOffset;

    public Transform spawnPoint;

    private float nextTimeToFire = 0f;
    public bool isEquipped = false;
    public bool hasEnemyEquipped = false;
    public bool canShoot = false;

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire && isEquipped)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot(false);
        }

        if(hasEnemyEquipped && canShoot && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot(true);
        }

    }

    void Shoot(bool isEnemy)
    {
        upperArm.AddForce(-upperArm.transform.forward * recoil, ForceMode.Impulse);

        if (!isEnemy)
        {
            target = autoAim.target.transform;
        }
        else
        {
            target = enemyAutoAim.aimAt;
        }

        GameObject bullet = Instantiate(projectile, spawnPoint.position, Quaternion.LookRotation(target.position - transform.position));
        bullet.GetComponent<BulletSystem>().Setup(projectileSpeed, bullet.transform.forward, hitForce, transform.root);
    }

}
