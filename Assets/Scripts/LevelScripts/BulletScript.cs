using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

	public float speed = 1f;
	public int damage = 5;

	public UnitScript target {
		set {
				target = value;
				gameObject.SetActive (value != null);
		}

//		get { return target; }
	}

	void Update ()
	{/*
		transform.position = Vector3.MoveTowards (transform.position, target.transform.position, speed * Time.deltaTime);
		if (Vector3.Distance (transform.position, target.transform.position) < 1) {
			Destroy (gameObject);
			//target.SetDamage (damage);
		}*/
	}
}
