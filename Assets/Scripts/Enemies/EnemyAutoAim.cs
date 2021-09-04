using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAutoAim : MonoBehaviour
{
    public GameObject[] weapons;

    public Transform hand;
    public Transform currentGunHeld;

    public bool canAim = true;
    public static Transform aimAt;
    public Transform hips;

    public LayerMask groundMask;

    public float rotationForce;
    [SerializeField] float rotationBalanceForce;
    public float drag;

    Rigidbody rb;

    GunSystem currentGunSystem;

    // Start is called before the first frame update
    void Start()
    {
        if(aimAt is null)
        {
            aimAt = GameObject.FindGameObjectWithTag("PlayerHips").transform;
        }

        weapons = Resources.LoadAll<GameObject>("Prefabs/Weapons");

        rb = GetComponent<Rigidbody>();

        currentGunHeld = Instantiate(weapons[Random.Range(0, weapons.Length)]).transform;

        currentGunSystem = currentGunHeld.GetComponent<GunSystem>();

        currentGunSystem.hasEnemyEquipped = true;
        currentGunHeld.GetComponent<Rigidbody>().isKinematic = true;
        currentGunSystem.upperArm = rb;
        currentGunSystem.enemyAutoAim = gameObject.GetComponent<EnemyAutoAim>();

        currentGunHeld.parent = hand;

        currentGunHeld.position = hand.position;
        currentGunHeld.localRotation = Quaternion.LookRotation(Vector3.right, -Vector3.right);
    }
        
    void FixedUpdate()
    {
        RaycastHit hit;
        if(Physics.Raycast(hips.position, aimAt.position - hips.position, out hit, Mathf.Infinity, groundMask) && canAim)
        {
            if(hit.transform == aimAt)
            {
                Debug.Log(hit.transform);

                rb.AddTorque(-rb.angularVelocity * rotationBalanceForce, ForceMode.Acceleration);

                Vector3 targetDelta = aimAt.position - transform.position;

                Vector3 cross = Vector3.Cross(transform.forward, targetDelta);

                rb.AddTorque(cross * rotationForce, ForceMode.Acceleration);

                currentGunSystem.canShoot = true;
            }
            else
            { 
                currentGunSystem.canShoot = false;
            }
        }
    }
}
