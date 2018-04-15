using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletScript : MonoBehaviour
{

	public float speed = 100f;
	public int damage = 5;
	public int liveTime = 2;

	private Vector3 forward = new Vector3 (0,0,1);

	void Start ()
	{
		StartCoroutine (Live ());
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.GetComponent<TowerScript> () != null)
			return;
		TowerScript tower = other.GetComponentInParent<TowerScript> ();
		if (tower != null) {
			tower.SetDamage (damage);
			Destroy (gameObject);
		}
	}

	void Update ()
	{ 
		Debug.DrawRay (transform.position, transform.forward);
		transform.Translate (forward*speed*Time.deltaTime);
	}

	IEnumerator Live ()
	{
		yield return new WaitForSeconds (liveTime);
		Destroy (gameObject);
	}
}
