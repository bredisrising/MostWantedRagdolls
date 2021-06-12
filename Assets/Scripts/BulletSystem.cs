using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSystem : MonoBehaviour
{

    float bulletSpeed;
    float hitForce;
    Vector3 bulletDirection;

    bool isDead;
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
        if(other.attachedRigidbody != null && other.transform.root.name != "Player")
        {
            if(other.transform.tag == "Enemy")
            {
                DefaultEnemyController controller = other.transform.root.GetComponentInChildren<DefaultEnemyController>();
                if (!controller.isDead)
                {
                    //controller.Die(true);
                }
                controller.transform.GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * hitForce, ForceMode.Acceleration);
            }
            else if(other.transform.tag == "Billy")
            {
                BillyController controller = other.transform.root.GetComponentInChildren<BillyController>();
                if (controller.isGrounded)
                {
                    controller.Die();
                    controller.transform.GetComponent<Rigidbody>().AddForce(bulletDirection.normalized * hitForce, ForceMode.Acceleration);
                }
            }
            else
            {
                other.attachedRigidbody.AddForce(bulletDirection.normalized * hitForce, ForceMode.Acceleration);
            }

            Destroy(this.gameObject);
        }
    }
}
