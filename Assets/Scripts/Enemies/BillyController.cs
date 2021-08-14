using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillyController : MonoBehaviour
{
    public Transform followObj;
    public Transform hips;
    public Transform homesParent;
    public Transform polesParent;

    public Transform leftFoot;
    public Transform rightFoot;

    public float higherGroundCheckDist;
    public float addedOtherCheckDist;
    public float groundCheckDist;

    InverseKinematics leftIK;
    InverseKinematics rightIK;
    ProceduralAnimation leftAnim;
    ProceduralAnimation rightAnim;

    Rigidbody hipsRb;

    LayerMask groundMask;

    public float speed;
    public float impactForceThreshold;

    public bool isGrounded = false;
    bool isStandingUp = false;
    bool doPushUp = false;
    public bool alternateLegs;

    public ConfigurableJoint[] cjs;
    JointDrive[] jds;
    JointDrive inAirDrive;

    public float airSpring;
    public float cfForce;
    public float rotationForce;
    public float rotationBalanceForce;


    private void Start()
    {
        jds = new JointDrive[cjs.Length];

        inAirDrive.maximumForce = Mathf.Infinity;
        inAirDrive.positionSpring = airSpring;


        hipsRb = GetComponent<Rigidbody>();

        leftIK = leftFoot.gameObject.GetComponent<InverseKinematics>();
        rightIK = rightFoot.gameObject.GetComponent<InverseKinematics>();
        leftAnim = leftFoot.gameObject.GetComponent<ProceduralAnimation>();
        rightAnim = rightFoot.gameObject.GetComponent<ProceduralAnimation>();

        //Saves the initial drives of each configurable joint
        for(int i = 0; i < cjs.Length; i++)
        {
            jds[i] = cjs[i].angularXDrive;
        }

        groundMask = LayerMask.GetMask("Ground");

        if (!alternateLegs)
        {
            StartCoroutine(LegUpdate());
        }
        else
        {
            StartCoroutine(AlternatingLegUpdate());
        }

    }
    void Update()
    {
        GroundHomeParent();
    }

    private void FixedUpdate()
    {
        CheckGrounded();

        if (isGrounded)
        {
            StabilizeBody();
            Move();
        }
    }

    void GroundHomeParent()
    {
        homesParent.position = new Vector3(hips.position.x, hips.position.y, hips.position.z);
        homesParent.eulerAngles = new Vector3(homesParent.eulerAngles.x, hips.eulerAngles.y, homesParent.eulerAngles.z);

        polesParent.position = new Vector3(hips.position.x, hips.position.y, hips.position.z);
        polesParent.eulerAngles = new Vector3(polesParent.eulerAngles.x, hips.eulerAngles.y, polesParent.eulerAngles.z);
    }

    void StabilizeBody()
    {
        hipsRb.AddTorque(-hipsRb.angularVelocity * rotationBalanceForce, ForceMode.Acceleration);
        var rot = Quaternion.FromToRotation(-transform.right, Vector3.right);
        hipsRb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * rotationBalanceForce, ForceMode.Acceleration);
    }
    void Move ()
    { 
        if(Vector3.Distance(transform.position, followObj.position) > 1.5)
        {
            Vector3 move = (followObj.position - transform.position).normalized;

            hipsRb.velocity = new Vector3(move.x * speed, hipsRb.velocity.y, move.z * speed);


            float rootAngle = transform.eulerAngles.y;
            float desiredAngle = Quaternion.LookRotation(followObj.position - transform.position).eulerAngles.y;

            float deltaAngle = Mathf.DeltaAngle(rootAngle, desiredAngle);

            hipsRb.AddTorque(Vector3.up * deltaAngle * rotationForce, ForceMode.Acceleration);
        }
        else
        {
            
            hipsRb.velocity = new Vector3(0, hipsRb.velocity.y, 0);

            float rootAngle = transform.eulerAngles.y;
            float desiredAngle = Quaternion.LookRotation(followObj.position - transform.position).eulerAngles.y;

            float deltaAngle = Mathf.DeltaAngle(rootAngle, desiredAngle);

            hipsRb.AddTorque(Vector3.up * deltaAngle * rotationForce, ForceMode.Acceleration);
        }
        
        
    }

    void CheckGrounded()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, Vector3.down, out hit, higherGroundCheckDist, groundMask))
        {
            if (!isGrounded)
            {
                StartCoroutine(DelayBeforeStand(3));
            }
            
            
            if (!isStandingUp)
            {
                if (Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDist, groundMask) && !doPushUp)
                {
                    hipsRb.AddForce(new Vector3(0, cfForce, 0), ForceMode.Acceleration);
                    doPushUp = true;
                }else if(Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckDist + addedOtherCheckDist, groundMask) && doPushUp)
                {
                    hipsRb.AddForce(new Vector3(0, cfForce, 0), ForceMode.Acceleration);
                }
                else
                {
                    doPushUp = false;
                    hipsRb.AddForce(new Vector3(0, 25, 0));
                }
            }

        }
        else
        {
            if (isGrounded)
            {
                Die();
            }
        }

        
          
    }

    public void Die()
    {
        rightIK.enabled = false;
        leftIK.enabled = false;
        isGrounded = false;

        foreach (ConfigurableJoint cj in cjs)
        {
            cj.angularXDrive = inAirDrive;
            cj.angularYZDrive = inAirDrive;
        }

        //hipsCj.angularYZDrive = inAirDrive;
        //hipsCj.angularXDrive = inAirDrive;

        
    }
    void SetDrives()
    {
        for(int i = 0; i < cjs.Length; i++)
        {
            cjs[i].angularXDrive = jds[i];
            cjs[i].angularYZDrive = jds[i];

        }

        rightIK.enabled = true;
        leftIK.enabled = true;
        isGrounded = true;
    }

    IEnumerator AlternatingLegUpdate()
    {
        while (true)
        {
            do
            {
                leftAnim.TryMove();
                yield return null;

            } while (leftAnim.moving);

            do
            {
                rightAnim.TryMove();
                yield return null;

            } while (rightAnim.moving);
        }
    }

    IEnumerator LegUpdate()
    {
        while (true)
        {
            do
            {
                leftAnim.TryMove();
                rightAnim.TryMove();
                yield return null;
            } while (rightAnim.moving);
        }
    }

    IEnumerator DelayBeforeStand(float delay)
    {
        isStandingUp = true;
        yield return new WaitForSeconds(delay);

        SetDrives();
        isStandingUp = false;
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag != "Projectile")
        {
            if(collision.relativeVelocity.magnitude > impactForceThreshold)
            {
                Die();
            }
        }
    }
}



