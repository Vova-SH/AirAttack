using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour {
	private UnitScript target;
	public float speed = 1f;
	public int damage = 1;

	public static void Create(UnitScript target){
		
		//this.target = target;
	}

	void Start () {
		if (target == null)
			Debug.Log ("Oh no");
	}


	void Update () {
		transform.position = Vector3.MoveTowards (transform.position, target.transform.position, speed * Time.deltaTime);
		if (Vector3.Distance (transform.position, target.transform.position) < 1) {
			//target.SetDamage (damage);
			Destroy (gameObject);
		}
	}
}
