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

    private void Start()
    {
        thisCollider = GetComponent<Collider>();
    }

    public void Setup(float bulletSpeed, Vector3 bulletDirection, float hitForce)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletDirection = bulletDirection;
        this.hitForce = hitForce;
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
        if(other.transform.root.name != "Player")
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
                    //Debug.Log("Hit!  " + other.transform.name);
                }
                else if (other.transform.tag == "Billy")
                {
                    BillyController controller = other.transform.root.GetComponentInChildren<BillyController>();
                    if (controller.isGrounded)
                    {
                        controller.Die();
                        controller.transform.GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                    }
                }
                else
                {
                    if (other.transform.tag == "Weapon")
                    {
                        other.attachedRigidbody.AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                    }

                }
            }
            
        }
    }
}
