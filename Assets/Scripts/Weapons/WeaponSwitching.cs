using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public Transform currentGunHeld;
    void Update()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.layer == 8)
                { 
                    if (currentGunHeld != null)
                    {
                        currentGunHeld.GetComponent<GunSystem>().isEquipped = false;
                        currentGunHeld.GetComponent<Rigidbody>().isKinematic = false;
                        currentGunHeld.parent = null;
                    }

                    currentGunHeld = hit.collider.gameObject.transform.root;
                    
                    currentGunHeld.GetComponent<GunSystem>().isEquipped = true;
                    currentGunHeld.GetComponent<Rigidbody>().isKinematic = true;
                    
                    currentGunHeld.parent = transform;

                    currentGunHeld.position = transform.position;
                    currentGunHeld.localRotation = Quaternion.LookRotation(Vector3.right, -Vector3.right);

                    //currentGunHeld.GetChild(0).transform.position = currentGunHeld.position;



                }
            }
        }
        
    }
}