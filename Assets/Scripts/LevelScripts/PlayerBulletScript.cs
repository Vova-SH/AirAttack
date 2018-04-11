using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletScript : MonoBehaviour
{

	public float speed = 100f;
	public int damage = 5;
	public int liveTime = 2;

	void Start ()
	{
		StartCoroutine ("Live");
	}

	void OnTriggerEnter (Collider other)
	{
		TowerScript tower = other.gameObject.GetComponent<TowerScript> ();
		if (tower != null) {
			tower.SetDamage (damage);
		}
		Destroy (gameObject);
	}

	void Update ()
	{
		transform.position = Vector3.MoveTowards (transform.position, Vector3.forward, speed * Time.deltaTime);
	}

	IEnumerator Live ()
	{
		yield return new WaitForSeconds (liveTime);
		Destroy (gameObject);
	}
}
