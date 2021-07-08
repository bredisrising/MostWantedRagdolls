using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DefaultEnemyController : MonoBehaviour
{
    public Transform ObjToFollow;
    public Transform hips;
    public Transform homesParent;
    public Transform polesParent;

    public Transform leftFoot;
    public Transform rightFoot;

    public float feetGroundCheckDist;

    InverseKinematics leftIK;
    InverseKinematics rightIK;
    ProceduralAnimation leftAnim;
    ProceduralAnimation rightAnim;

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
    public float force;

    bool isGrounded = true;
    public bool isDead;
    bool alternateLegs = true;

    public ConfigurableJoint[] cjs;
    JointDrive[] jds;
    JointDrive inAirDrive;
    JointDrive hipsInAirDrive;

    NavMeshAgent navMeshAgent;

    Vector3 currentTargetPos;
    
    private void Start()
    {
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
        GroundHomeParent();
        CheckGrounded();
    }

    void GroundHomeParent()
    {
        homesParent.position = new Vector3(hips.position.x, homesParent.position.y, hips.position.z);
        homesParent.eulerAngles = new Vector3(homesParent.eulerAngles.x, hips.eulerAngles.y, homesParent.eulerAngles.z);

        polesParent.position = new Vector3(hips.position.x, polesParent.position.y, hips.position.z);
        polesParent.eulerAngles = new Vector3(polesParent.eulerAngles.x, hips.eulerAngles.y, polesParent.eulerAngles.z);
    }

    void StabilizeBody()
    {
        headRb.AddForce(Vector3.up * force);
        hipsRb.AddForce(Vector3.down * force);
        hipsRb.AddTorque(-hipsRb.angularVelocity * rotationForce, ForceMode.Acceleration);
    }

    void Move ()
    {
        if (Vector3.Distance(transform.position, ObjToFollow.position) > 4)
        {
            if (Vector3.Distance(transform.position, currentTargetPos) <= .5f)
            { 
                currentTargetPos = FindNextTargetPosOnPath();
            }
            else
            {
                Vector3 move = (currentTargetPos - transform.position).normalized;

                //hipsRb.velocity = new Vector3(move.x * speed, hipsRb.velocity.y, move.z * speed);
                //torsoRb.velocity = new Vector3(move.x * speed, torsoRb.velocity.y, move.z * speed);

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

            Vector3 locVel = hipsRb.velocity;
            locVel.z = 0;
            locVel.x = 0;
            hipsRb.velocity = locVel;
            torsoRb.velocity = locVel;

            float rootAngle = transform.eulerAngles.y;
            float desiredAngle = Quaternion.LookRotation(ObjToFollow.position - transform.position).eulerAngles.y;

            float deltaAngle = Mathf.DeltaAngle(rootAngle, desiredAngle);

            hipsRb.AddTorque(Vector3.up * deltaAngle * rotationForce, ForceMode.Acceleration);
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
            leftCheck = true;

        if (Physics.Raycast(rightFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
            rightCheck = true;

        if ((rightCheck || leftCheck) && !isGrounded)
        {
            StartCoroutine(DelayBeforeStand(3));
        }
        else if((!rightCheck && !leftCheck) && isGrounded)
        {
            Die(false);
        }
            
        

    }

    public void Die(bool noRespawn)
    {
        if (!noRespawn)
        {
            foreach (ConfigurableJoint cj in cjs)
            {
                cj.angularXDrive = inAirDrive;
                cj.angularYZDrive = inAirDrive;
            }

            hipsCj.angularYZDrive = hipsInAirDrive;
            hipsCj.angularXDrive = hipsInAirDrive;

            rightIK.enabled = false;
            leftIK.enabled = false;
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

            rightIK.enabled = false;
            leftIK.enabled = false;
            isGrounded = false;
            isDead = true;

            autoAim.enabled = false;

            Destroy(this);
        }
        
        
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
        isDead = false;
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
            } while (leftAnim.moving && rightAnim.moving);
        }
    }


    IEnumerator DelayBeforeStand(float delay)
    {
        yield return new WaitForSeconds(delay);
        SetDrives();
    }


}



