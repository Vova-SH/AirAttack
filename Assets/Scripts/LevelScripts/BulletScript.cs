using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

	public float speed = 1f;
	public int damage = 5;
	private UnitScript target;
	public GameObject player;

	public UnitScript Target {
		set {
			gameObject.SetActive (value != null);
			target = value;
		}

		get { return target; }
	}

	void Update ()
	{
		if (target != null) {
			transform.position = Vector3.MoveTowards (transform.position, target.transform.position, speed * Time.deltaTime);
			transform.rotation = Quaternion.LookRotation (player.transform.position);
			if (Vector3.Distance (transform.position, target.transform.position) < 1) {
				Destroy (gameObject);
				target.SetDamage (damage);
			}
		} else Destroy (gameObject);
	}
}
