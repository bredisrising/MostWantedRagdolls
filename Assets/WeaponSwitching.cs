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
                    Debug.LogWarning("jo biden");

                    if (currentGunHeld != null)
                    {
                        currentGunHeld.GetComponent<GunSystem>().isEquipped = false;
                        currentGunHeld.GetComponent<Rigidbody>().isKinematic = false;
                        currentGunHeld.parent.parent = null;
                    }

                    currentGunHeld = hit.collider.gameObject.transform;
                    currentGunHeld.GetComponent<Rigidbody>().isKinematic = true;
                    currentGunHeld.GetComponent<GunSystem>().isEquipped = true;
                    currentGunHeld.parent = transform;
                    currentGunHeld.parent.transform.position = transform.position;
                }
            }
        }
        
    }
}
