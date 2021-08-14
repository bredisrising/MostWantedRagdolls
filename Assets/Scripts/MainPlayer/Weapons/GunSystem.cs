using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GunSystem : MonoBehaviour
{
    [HideInInspector] public Rigidbody upperArm;
    [HideInInspector] public AutoAim autoAim;
    [HideInInspector] public EnemyAutoAim enemyAutoAim;

    [SerializeField] float recoil;
    [SerializeField] float hitForce;
    [SerializeField] float bloom;
    [SerializeField] float ammoCount;
    [SerializeField] float fireRate;
    [SerializeField] float projectileSpeed;
    [SerializeField] float playerProjectileSpeedMultiplier;
    private float playerProjectileSpeed;
    [SerializeField] float reloadSpeed;

    [SerializeField] GameObject playerProjectile;
    [SerializeField] GameObject enemyProjectile;

    [SerializeField] Transform spawnPoint;
    Transform target;

    private float nextTimeToFire = 0f;
    public float currentAmmo;
    public bool isEquipped = false;
    public bool hasEnemyEquipped = false;
    public bool canShoot = false;

    TextMeshProUGUI ammoText;
    Image reloadProgress;

    private void Start()
    {
        playerProjectileSpeed = projectileSpeed * playerProjectileSpeedMultiplier;
        currentAmmo = ammoCount;
    }

    public void Equip(TextMeshProUGUI text, Image image)
    {
        ammoText = text;
        reloadProgress = image;

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
        if (isEquipped)
        {
            ammoText.text = currentAmmo.ToString();
        }

    }

    void Shoot(bool isEnemy)
    {

        if (!isEnemy)
        {
            target = autoAim.target.transform;
            upperArm.AddForce(-upperArm.transform.forward * recoil, ForceMode.Impulse);

            GameObject bullet = Instantiate(playerProjectile, spawnPoint.position, Quaternion.LookRotation((target.position) - transform.position));
            bullet.GetComponent<BulletSystem>().Setup(playerProjectileSpeed, bullet.transform.forward, hitForce, transform.root);
        }
        else
        {
            target = EnemyAutoAim.aimAt;
            GameObject bullet = Instantiate(enemyProjectile, spawnPoint.position, Quaternion.LookRotation((target.position) - transform.position));
            bullet.GetComponent<BulletSystem>().Setup(projectileSpeed, bullet.transform.forward, hitForce, transform.root);
        }

        
    }

}
