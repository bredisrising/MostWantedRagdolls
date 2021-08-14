using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicExplosion : MonoBehaviour
{
	[SerializeField] float radius, power;
	private void Start()
	{
		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);

		GameManager.Instance.DoSlowmotion();

		foreach (Collider hit in colliders)
		{
			Rigidbody rb = hit.GetComponent<Rigidbody>();

			if (rb != null)
			{
				if(rb.CompareTag("PlayerHips"))
				{
					Debug.Log("Oof");
					rb.AddExplosionForce(power, explosionPos, radius, .75f, ForceMode.VelocityChange);
					rb.AddTorque(new Vector3(Random.Range(-25, 25), Random.Range(-25, 25), Random.Range(-25, 25)), ForceMode.VelocityChange);
					rb.GetComponent<PlayerController>().Die(false);
				}
					
			}
		}
		
	}

}

