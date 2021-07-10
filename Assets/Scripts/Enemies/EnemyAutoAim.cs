using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAutoAim : MonoBehaviour
{
    public GameObject[] weapons;

    public Transform hand;
    public Transform currentGunHeld;

    public bool canAim = true;
    public Transform aimAt;
    public Transform hips;

    public LayerMask groundMask;

    public float rotationForce;
    public float drag;

    bool canSeePlayer;
    Rigidbody rb;

    GunSystem currentGunSystem;

    // Start is called before the first frame update
    void Start()
    {
        weapons = Resources.LoadAll<GameObject>("Prefabs/Weapons");

        rb = GetComponent<Rigidbody>();

        currentGunHeld = Instantiate(weapons[Random.Range(0, weapons.Length - 1)]).transform;

        currentGunSystem = currentGunHeld.GetComponent<GunSystem>();

        currentGunSystem.hasEnemyEquipped = true;
        currentGunHeld.GetComponent<Rigidbody>().isKinematic = true;
        currentGunSystem.upperArm = rb;
        currentGunSystem.enemyAutoAim = gameObject.GetComponent<EnemyAutoAim>();

        currentGunHeld.parent = hand;

        currentGunHeld.position = hand.position;
        currentGunHeld.localRotation = Quaternion.LookRotation(Vector3.right, -Vector3.right);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        if(Physics.Raycast(hips.position, aimAt.position - hips.position, out hit, Mathf.Infinity, groundMask) && canAim)
        {
            if(hit.transform == aimAt)
            {
                Debug.Log("Enemy Spotted!");

                rb.angularDrag = drag;
                Vector3 targetDelta = aimAt.position - transform.position;

                float angleDiff = Vector3.Angle(transform.forward, targetDelta);

                Vector3 cross = Vector3.Cross(transform.forward, targetDelta);

                rb.AddTorque(cross * rotationForce, ForceMode.Acceleration);


                currentGunSystem.canShoot = true;

                //Debug.DrawLine(transform.position, aimAt.position, Color.red);
            }
            else
            {
                if (rb.angularDrag != 0.05)
                    rb.angularDrag = 0.05f;

                currentGunSystem.canShoot = false;
            }
        }
    }
}
