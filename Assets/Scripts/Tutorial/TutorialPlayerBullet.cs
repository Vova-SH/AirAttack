using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPlayerBullet : MonoBehaviour
{

	public float speed = 100f;
	public int damage = 5;
	public int liveTime = 2;

	void Start ()
	{
		StartCoroutine (Live ());
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.GetComponent<Target> () != null)
			return;
		Target target = other.GetComponentInParent<Target> ();
		if (target != null) {
			target.SetDamage (damage);
			Destroy (gameObject);
		}
	}

	void Update ()
	{
		transform.position += transform.forward*speed*Time.deltaTime;
	}

	IEnumerator Live ()
	{
		yield return new WaitForSeconds (liveTime);
		Destroy (gameObject);
	}
}