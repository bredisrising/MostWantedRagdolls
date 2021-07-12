using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunSystem : MonoBehaviour
{
    public Rigidbody upperArm;
    public AutoAim autoAim;
    public EnemyAutoAim enemyAutoAim;

    public float recoil;
    public float hitForce;
    public float bloom;
    public float ammoCount;
    public float fireRate;
    public float projectileSpeed;
    public float reloadSpeed;

    public GameObject projectile;
    public Transform spawnPoint;
    Transform target;

    private float nextTimeToFire = 0f;
    public float currentAmmo;
    public bool isEquipped = false;
    public bool hasEnemyEquipped = false;
    public bool canShoot = false;

    public TextMeshProUGUI ammoText;
    public Image reloadProgress;

    private void Start()
    {
        currentAmmo = ammoCount;
    }

    public void Equip()
    {
        ammoText.text = currentAmmo.ToString();
        reloadProgress.fillAmount = currentAmmo / ammoCount;
    }

    void Update()
    {
        if (Input.GetMouseButton(0) && Time.time >= nextTimeToFire && isEquipped && currentAmmo > 0)
        {
            currentAmmo--;
            ammoText.text = currentAmmo.ToString();
            reloadProgress.fillAmount = currentAmmo / ammoCount;
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot(false);
        }

        if(hasEnemyEquipped && canShoot && Time.time >= nextTimeToFire && currentAmmo > 0)
        {
            currentAmmo--;
            nextTimeToFire = Time.time + 1f / fireRate;
            Shoot(true);
        }

        if(currentAmmo <= 0)
        {
            Invoke("Reload", reloadSpeed);
        }

    }


    void Reload()
    {
        currentAmmo = ammoCount;
        ammoText.text = currentAmmo.ToString();
    }

    void Shoot(bool isEnemy)
    {

        if (!isEnemy)
            upperArm.AddForce(-upperArm.transform.forward * recoil, ForceMode.Impulse);

        if (!isEnemy)
        {
            target = autoAim.target.transform;
        }
        else
        {
            target = enemyAutoAim.aimAt;
        }

        GameObject bullet = Instantiate(projectile, spawnPoint.position, Quaternion.LookRotation((target.position) - transform.position));
        bullet.GetComponent<BulletSystem>().Setup(projectileSpeed, bullet.transform.forward, hitForce, transform.root);
    }

}
