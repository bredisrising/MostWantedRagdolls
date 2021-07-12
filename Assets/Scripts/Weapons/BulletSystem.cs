using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSystem : MonoBehaviour
{

    float bulletSpeed;
    float hitForce;
    Vector3 bulletDirection;

    Collider thisCollider;

    bool isDead;

    Transform whoSpawnedMe;

    private void Start()
    {
        thisCollider = GetComponent<Collider>();
    }

    public void Setup(float bulletSpeed, Vector3 bulletDirection, float hitForce, Transform whoSpawnedMe)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletDirection = bulletDirection;
        this.hitForce = hitForce;
        this.whoSpawnedMe = whoSpawnedMe;
    }

    private void Update()
    {
        if (!isDead)
        {
            transform.position += bulletDirection * bulletSpeed * Time.deltaTime;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root != whoSpawnedMe)
        {
            thisCollider.enabled = false;
            Destroy(gameObject);
            if(other.attachedRigidbody != null)
            {
                if (other.transform.tag == "Enemy")
                {
                    DefaultEnemyController controller = other.transform.root.GetComponentInChildren<DefaultEnemyController>();
                    if (!controller.isDead)
                    {
                        //controller.Die(true);
                    }
                    controller.transform.GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                }
                else if (other.transform.tag == "Billy")
                {
                    BillyController controller = other.transform.root.GetComponentInChildren<BillyController>();
                    if (controller.isGrounded)
                    {
                        controller.Die();
                        
                    }
                    controller.transform.GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                }
                else if (other.transform.root.GetComponentInChildren<PlayerController>())
                {
                    PlayerController controller = other.transform.root.GetComponentInChildren<PlayerController>();
                    controller.transform.GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                }
                else
                {
                    if (other.transform.tag != "Weapon")
                    {
                        other.attachedRigidbody.AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                    }

                }

              

            }
            
        }
    }
}
