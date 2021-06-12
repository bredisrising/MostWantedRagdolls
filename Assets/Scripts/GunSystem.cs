using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSystem : MonoBehaviour
{
    public Rigidbody upperArm;
    public AutoAim autoAim;

    public float recoil;
    public float hitForce;
    public float jumpForce;

    public float fireRate;

    public GameObject projectile;
    public float projectileSpeed;
    public float spawnOffset;

    public Transform spawnPoint;

    private float nextTimeToFire = 0f;
    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        upperArm.AddForce(-upperArm.transform.forward * recoil, ForceMode.Impulse);

        GameObject bullet = Instantiate(projectile, spawnPoint.position, Quaternion.LookRotation(autoAim.target.transform.position - spawnPoint.position));
        
        bullet.GetComponent<BulletSystem>().Setup(projectileSpeed, bullet.transform.forward, hitForce);
    }
}
