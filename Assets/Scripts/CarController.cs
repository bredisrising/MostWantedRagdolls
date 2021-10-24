using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentBrakeForce;
    private bool isBraking;
    [SerializeField] Rigidbody rb;

    [SerializeField] float yCenterOfMass;
    [SerializeField] float hitForce;

    [SerializeField] float motorForce;
    [SerializeField] float brakeForce;
    [SerializeField] float maxSteeringAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider;
    [SerializeField] private WheelCollider frontRightWheelCollider;
    [SerializeField] private WheelCollider backLeftWheelCollider;
    [SerializeField] private WheelCollider backRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform;
    [SerializeField] private Transform frontRightWheelTransform;
    [SerializeField] private Transform backLeftWheelTransform;
    [SerializeField] private Transform backRightWheelTransform;

    private void Start()
    {
        //Rigidbody rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, yCenterOfMass, 0);
    }

    private void Update()
    {
        GetInput();

        UpdateWheels();
    }
    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();

    }

    private void HandleMotor()
    {
        backLeftWheelCollider.motorTorque = verticalInput * motorForce;
        backRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        frontLeftWheelCollider.brakeTorque = currentBrakeForce;
        frontRightWheelCollider.brakeTorque = currentBrakeForce;
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = -Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteeringAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(backLeftWheelCollider, backLeftWheelTransform);
        UpdateSingleWheel(backRightWheelCollider, backRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "EnemyCollider")
        {
            DefaultEnemyController controller = other.transform.root.GetComponentInChildren<DefaultEnemyController>();
            if (!controller.isDead)
            {
                controller.Die(true);
            }
            Vector3 force = rb.velocity.normalized * hitForce;
            other.transform.root.Find("Torso").GetComponent<Rigidbody>().AddForce(new Vector3(force.x, force.y * 5, force.z), ForceMode.VelocityChange);
        }
    }
}
