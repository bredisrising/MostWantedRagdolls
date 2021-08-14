using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponSwitching : MonoBehaviour
{
    public Transform currentGunHeld;

    TextMeshProUGUI playerAmmoText;
    Image playerAmmoProgressCr;

    private void Start()
    {
        playerAmmoProgressCr = GameObject.Find("Radial").GetComponent<Image>();
        playerAmmoText = GameObject.Find("AmmoCount").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(cameraRay, out hit, Mathf.Infinity))
            {
                if (hit.collider.gameObject.layer == 8 && !hit.collider.gameObject.GetComponent<GunSystem>().hasEnemyEquipped)
                {
                    if(!hit.collider.gameObject.GetComponent<GunSystem>().isEquipped || !hit.collider.gameObject.GetComponent<GunSystem>().hasEnemyEquipped)
                    {
                        if (currentGunHeld != null)
                        {
                            currentGunHeld.GetComponent<GunSystem>().isEquipped = false;
                            currentGunHeld.GetComponent<Rigidbody>().isKinematic = false;
                            currentGunHeld.parent = null;
                        }

                        currentGunHeld = hit.collider.gameObject.transform.root;

                        GunSystem currentGunSystem = currentGunHeld.GetComponent<GunSystem>();

                        currentGunSystem.isEquipped = true;
                        currentGunHeld.GetComponent<Rigidbody>().isKinematic = true;
                        currentGunSystem.upperArm = transform.parent.parent.GetComponent<Rigidbody>(); ;

                        currentGunHeld.parent = transform;

                        currentGunHeld.position = transform.position;
                        currentGunHeld.localRotation = Quaternion.LookRotation(Vector3.right, -Vector3.right);

                        currentGunSystem.Equip(playerAmmoText, playerAmmoProgressCr);
                    }
                   

                }
            }
        }
        
    }
}
