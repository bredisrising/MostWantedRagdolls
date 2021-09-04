using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DefaultEnemyController : MonoBehaviour
{
    public static Transform ObjToFollow;
    public Transform hips;

    public Transform leftFoot;
    public Transform rightFoot;

    [SerializeField] ProceduralLegsController proceduralLegs;

    public float feetGroundCheckDist;

    public Rigidbody torsoRb;
    public Rigidbody headRb;

    ConfigurableJoint hipsCj;
    Rigidbody hipsRb;

    public AutoAim autoAim;

    LayerMask groundMask;

    public float speed;
    public float maxVelocityChange;
    public float airSpring;
    public float rotationForce;
    [SerializeField] float idleRotationForce;
    public float rotationBalanceForce;
    public float balanceForce;

    [SerializeField] bool isGrounded = true;
    bool standing = false;
    public bool isDead;
    [SerializeField] bool respawnWhenDead = true;

    public ConfigurableJoint[] cjs;
    JointDrive[] jds;
    JointDrive inAirDrive;
    JointDrive hipsInAirDrive;

    NavMeshAgent navMeshAgent;

    Vector3 currentTargetPos;
    
    private void Start()
    {
        if(ObjToFollow is null)
        {
            //ObjToFollow = GameObject.FindGameObjectWithTag("PlayerHips").transform;
            GameObject[] test = GameObject.FindGameObjectsWithTag("PlayerHips");
            if(test.Length > 0)
            {
                ObjToFollow = GameObject.FindGameObjectsWithTag("PlayerHips")[0].transform;
            }

        }

        jds = new JointDrive[cjs.Length];

        inAirDrive.maximumForce = Mathf.Infinity;
        inAirDrive.positionSpring = airSpring;

        hipsInAirDrive.maximumForce = Mathf.Infinity;
        hipsInAirDrive.positionSpring = 0;

        navMeshAgent = GetComponent<NavMeshAgent>();

        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;

        hipsRb = GetComponent<Rigidbody>();
        hipsCj = GetComponent<ConfigurableJoint>();

        //Saves the initial drives of each configurable joint
        for(int i = 0; i < cjs.Length; i++)
        {
            jds[i] = cjs[i].angularXDrive;
        }

        groundMask = LayerMask.GetMask("Ground");

        if(ObjToFollow != null)
            currentTargetPos = FindNextTargetPosOnPath();
    }
    private void FixedUpdate()
    {
        if (isGrounded)
        {
            StabilizeBody();
            Move();
        }
    }
    void Update()
    {
        CheckGrounded();
        proceduralLegs.GroundHomeParent();
    }
    void StabilizeBody()
    {
        headRb.AddForce(Vector3.up * balanceForce);
        hipsRb.AddForce(Vector3.down * balanceForce);
        hipsRb.AddTorque(-hipsRb.angularVelocity * rotationBalanceForce, ForceMode.Acceleration);
    }
    void Move ()
    {
        if (ObjToFollow != null && Vector3.Distance(transform.position, ObjToFollow.position) > 4 && navMeshAgent.pathStatus == NavMeshPathStatus.PathInvalid)
        {

            if (Vector3.Distance(transform.position, currentTargetPos) <= .5f)
            { 
                currentTargetPos = FindNextTargetPosOnPath();
            }
            else
            {
                Vector3 move = (currentTargetPos - transform.position).normalized;

                Vector3 targetVelocity = new Vector3(move.x, 0, move.z);
                targetVelocity *= speed;

                Vector3 velocity = hipsRb.velocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;
                hipsRb.AddForce(velocityChange, ForceMode.VelocityChange);

                float rootAngle = transform.eulerAngles.y;
                float desiredAngle = Quaternion.LookRotation(currentTargetPos - transform.position).eulerAngles.y;

                float deltaAngle = Mathf.DeltaAngle(rootAngle, desiredAngle);

                hipsRb.AddTorque(Vector3.up * deltaAngle * rotationForce, ForceMode.Acceleration);
            }
        }
        else
        {
            Vector3 targetVelocity = hipsRb.velocity.normalized * 1;
            Vector3 velocity = hipsRb.velocity;
            Vector3 velocityChange = targetVelocity - velocity;

            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;
            hipsRb.AddForce(velocityChange, ForceMode.VelocityChange);
            torsoRb.AddForce(velocityChange, ForceMode.VelocityChange);

            //

            float rootAngle = transform.eulerAngles.y;
            float desiredAngle = Quaternion.LookRotation(hipsRb.velocity).eulerAngles.y;

            float deltaAngle = Mathf.DeltaAngle(rootAngle, desiredAngle);

            hipsRb.AddTorque(Vector3.up * deltaAngle * idleRotationForce, ForceMode.Acceleration);
        }
    }
    Vector3 FindNextTargetPosOnPath()
    {
        navMeshAgent.enabled = true;

        if (navMeshAgent.isOnNavMesh)
        {
            NavMeshPath path = new NavMeshPath();
            if (navMeshAgent.CalculatePath(ObjToFollow.position, path))
            {
                navMeshAgent.path = path;
            }

        }
        Vector3 pos = navMeshAgent.steeringTarget;

        navMeshAgent.enabled = false;

        return pos;
    }

    void CheckGrounded()
    {
        bool leftCheck = false;
        bool rightCheck = false;
        RaycastHit hit;

        if (Physics.Raycast(leftFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
        {
            leftCheck = true;
        }
        if (Physics.Raycast(rightFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
        {
            rightCheck = true;
        }
            

        if ((rightCheck || leftCheck) && !isGrounded && !standing && respawnWhenDead)
        {
            StartCoroutine(DelayBeforeStand(3));
            Debug.Log("STANDING!");
        }
        else if((!rightCheck && !leftCheck) && isGrounded)
        {
            Die(respawnWhenDead);
            Debug.Log("DIED");
        }
    }
    public void Die(bool respawn)
    {
        if (respawn)
        {
            foreach (ConfigurableJoint cj in cjs)
            {
                cj.angularXDrive = inAirDrive;
                cj.angularYZDrive = inAirDrive;
            }

            hipsCj.angularYZDrive = hipsInAirDrive;
            hipsCj.angularXDrive = hipsInAirDrive;

            proceduralLegs.DisableIk();
            isGrounded = false;
            isDead = true;
        }
        else
        {
            foreach (ConfigurableJoint cj in cjs)
            {
                cj.angularXDrive = inAirDrive;
                cj.angularYZDrive = inAirDrive;
            }

            hipsCj.angularYZDrive = hipsInAirDrive;
            hipsCj.angularXDrive = hipsInAirDrive;

            proceduralLegs.DisableIk();
            isGrounded = false;
            isDead = true;

            //autoAim.enabled = false;

        }
    }
    void SetDrives()
    {
        for(int i = 0; i < cjs.Length; i++)
        {
            cjs[i].angularXDrive = jds[i];
            cjs[i].angularYZDrive = jds[i];
        }

        proceduralLegs.EnableIk();
        isGrounded = true;
        isDead = false;
    }
    IEnumerator DelayBeforeStand(float delay)
    {
        standing = true;
        yield return new WaitForSeconds(delay);
        hipsRb.AddForce(Vector3.up * 250);
        SetDrives();
        standing = false;

    }

}



