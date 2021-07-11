using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Transform hips;
    public Transform cam;
    public Transform homesParent;
    public Transform polesParent;

    public ConfigurableJoint torsoCj;

    public Transform leftFoot;
    public Transform rightFoot;

    public float feetGroundCheckDist;

    InverseKinematics leftIK;
    InverseKinematics rightIK;
    ProceduralAnimation leftAnim;
    ProceduralAnimation rightAnim;

    ConfigurableJoint hipsCj;
    Rigidbody hipsRb;

    public Rigidbody headRb;

    LayerMask groundMask;

    public float speed;
    public float maxVelocityChange;
    public float rotationForce;
    public float balanceForce;
    public float jumpForce;

    public bool isGrounded;
    public bool alternateLegs;

    float horizontal, vertical;

    public ConfigurableJoint[] cjs;
    JointDrive[] jds;
    JointDrive inAirDrive;
    JointDrive hipsInAirDrive;

    public float airSpring;
    private void Start()
    {
        jds = new JointDrive[cjs.Length];

        inAirDrive.maximumForce = Mathf.Infinity;
        inAirDrive.positionSpring = airSpring;

        hipsInAirDrive.maximumForce = Mathf.Infinity;
        hipsInAirDrive.positionSpring = 0;

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
        
    }
    void Update()
    {
        GroundHomeParent();
        CheckGrounded();
        SetPlayerInputs();

        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Jump();
        }
    }


    private void FixedUpdate()
    {
        if (isGrounded)
        {
            StabilizeBody();
            Move();
        }

        
    }

    void GroundHomeParent()
    {
        homesParent.position = new Vector3(hips.position.x, transform.position.y, hips.position.z);
        homesParent.eulerAngles = new Vector3(homesParent.eulerAngles.x, hips.eulerAngles.y, homesParent.eulerAngles.z);

        polesParent.position = new Vector3(hips.position.x, transform.position.y, hips.position.z);
        polesParent.eulerAngles = new Vector3(polesParent.eulerAngles.x, hips.eulerAngles.y, polesParent.eulerAngles.z);
    }

    void SetPlayerInputs()
    {
        
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
       
    }

    void StabilizeBody()
    {
        headRb.AddForce(Vector3.up * balanceForce);
        hipsRb.AddForce(Vector3.down * balanceForce);
    }

    void Move()
    {
        Vector3 move = new Vector3(horizontal, 0f, vertical);
        move = cam.TransformDirection(move);
        //move = new Vector3(move.x, 0, move.z).normalized * speed;
        //move = new Vector3(move.x, hipsRb.velocity.y, move.z);

        /*if (new Vector3(move.x, 0, move.z).magnitude > 0.01)
        {
            hipsRb.velocity = move;
            torsoCj.targetRotation = Quaternion.Euler(-5, 0, 0);
            playerRotation = new Vector3(move.x, 0f, move.z);
        }
        else
        {
            torsoCj.targetRotation = Quaternion.identity;
            hipsRb.velocity = move;
        }*/

        Vector3 targetVelocity = new Vector3(move.x, 0, move.z);
        targetVelocity *= speed;

        Vector3 velocity = hipsRb.velocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        hipsRb.AddForce(velocityChange, ForceMode.VelocityChange);

        float desiredAngle = 0;
        float rootAngle = transform.eulerAngles.y;

        if(targetVelocity.normalized != Vector3.zero)
        {
             desiredAngle = Quaternion.LookRotation(targetVelocity.normalized).eulerAngles.y;
        }

        float deltaAngle = Mathf.DeltaAngle(rootAngle, desiredAngle);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            hipsRb.AddTorque(Vector3.up * deltaAngle * rotationForce, ForceMode.Acceleration);
        }

        //Debug.Log(targetVelocity.magnitude);
        /*if (playerRotation != Vector3.zero)
            hips.rotation = Quaternion.LookRotation(playerRotation);*/


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
            SetDrives();
        }
        else if((!rightCheck && !leftCheck) && isGrounded)
        {
            Die();
        }
            
        

    }

    void Die()
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
    public void Jump()
    {
        
        hipsRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        hipsRb.AddTorque(new Vector3(10, 0 , 0));
        
        
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
}



