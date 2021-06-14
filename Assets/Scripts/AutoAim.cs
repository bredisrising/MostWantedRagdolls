using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    public bool isEnemy;

    public Camera cam;

    public GameObject aimTargets;
    public GameObject target;

    Vector3 aimTarget;

    public Transform hand;

    public LayerMask enemyMask;
    public LayerMask obstacleMask;

    public float speed;
    public float rotationForce;

    public Transform hips;

    public float radius;
    public float viewAngle;

    Rigidbody rb;

    public List<Transform> visibleTargets = new List<Transform>();

    public bool isAiming = false;

    Vector3 pointToLook;
    private void Start()
    {

        rb = GetComponent<Rigidbody>();

        target = new GameObject(this.name + " target");
        target.transform.parent = aimTargets.transform;

        if (!isEnemy)
        {
            StartCoroutine("FindTargetsWithDelay", .2f);
        }

    }
    void FixedUpdate()
    {
        if (!isEnemy)
        {
            Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity))
            {

                pointToLook = hit.point;
                //pointToLook = new Vector3(pointToLook.x, hips.position.y, pointToLook.z);
                Debug.DrawLine(cameraRay.origin, pointToLook, Color.cyan);

            }
        }
        

        float step = speed * Time.deltaTime;
        target.transform.position = Vector3.MoveTowards(target.transform.position, aimTarget, step);

        if (isAiming)
        {
            //transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position, -Vector3.right);
            //transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 90);

            Vector3 targetDelta = target.transform.position - transform.position;

            float angleDiff = Vector3.Angle(transform.forward, targetDelta);

            Vector3 cross = Vector3.Cross(transform.forward, targetDelta);

            rb.AddTorque(cross * angleDiff * rotationForce);

        }

    }

  

    void FindVisibleTargets()
    {
        visibleTargets.Clear();

        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, radius, enemyMask);

        for(int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - hips.position).normalized;
            if (Vector3.Angle((pointToLook - hips.position).normalized, dirToTarget) < viewAngle/2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add(target);
                }
            }
        }
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
            if (visibleTargets.Count > 0)
            {

                isAiming = true;

                aimTarget = visibleTargets[0].position;


            }
            else
            {

                isAiming = false;
                aimTarget = hand.position + hand.transform.forward;
                target.transform.position = aimTarget;
            }
        }
    }

}
