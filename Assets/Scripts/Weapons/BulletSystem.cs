using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSystem : MonoBehaviour
{
    float bulletSpeed;
    float hitForce;
    Vector3 bulletDirection;

    Collider thisCollider;
    Rigidbody rb;

    bool isDead;

    Transform whoSpawnedMe;

    private void Start()
    {
        thisCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }

    public void Setup(float bulletSpeed, Vector3 bulletDirection, float hitForce, Transform whoSpawnedMe)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletDirection = bulletDirection;
        this.hitForce = hitForce;
        this.whoSpawnedMe = whoSpawnedMe;
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            rb.MovePosition(transform.position + bulletDirection * bulletSpeed * Time.deltaTime);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //U SHOULD PROBABLY OPTIMIZE THIS!!!!!!

        Debug.Log(other.transform.name);

        if(other.transform.root != whoSpawnedMe && other.transform.tag != "Projectile")
        {

            //Debug.Log(other.transform.root.name);
            thisCollider.enabled = false;
            
            if(other.attachedRigidbody != null)
            {
                if (other.transform.tag == "EnemyCollider")
                { 
                    DefaultEnemyController controller = other.transform.root.GetComponentInChildren<DefaultEnemyController>();
                    if (!controller.isDead)
                    {
                        //controller.Die(true);
                    }
                    other.transform.root.Find("Torso").GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                }
                else if (other.transform.tag == "PlayerHips")
                {
                    //HEY! JUST A REMINDER
                    //OPTIMIZE THIS I THINK IDK THO
                    //MEBBE ITS GOOD
                    other.transform.root.Find("Torso").GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                }
                else
                {
                    if (other.transform.tag != "Weapon")
                    {
                        other.attachedRigidbody.AddForce(bulletDirection.normalized * hitForce, ForceMode.VelocityChange);
                    }

                }

                Destroy(gameObject);

            }

        }
    }
}
