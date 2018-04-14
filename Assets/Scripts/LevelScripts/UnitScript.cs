using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{

	private List<TowerScript> towers = new List<TowerScript> ();
	public int health = 100;
	public int armor = 0;
	public int speed = 5;
	public float angularVelocity = 80f;


	public Resistable[] resistance;

	private Vector3? target = null;
	private Vector3 delta;
	private MoveStatus callback = null;

	void Start ()
	{
		//TODO: add trigger, when shoot collide
	}

	void OnTriggerEnter (Collider other)
	{
		TowerScript tower = other.gameObject.GetComponent<TowerScript> ();
		if (tower != null) {
			towers.Add (tower);
		}
	}

	void OnTriggerExit (Collider other)
	{
		TowerScript tower = other.gameObject.GetComponent<TowerScript> ();
		if (tower != null) {
			towers.Remove (tower);
		}
	}

	void Update ()
	{
		if (target != null) {
			transform.Translate (transform.forward*speed*Time.deltaTime);
			Quaternion relativePos = Quaternion.LookRotation (target.Value - transform.position);
			Quaternion rotation = Quaternion.RotateTowards (transform.rotation, relativePos, angularVelocity * Time.deltaTime);
			transform.rotation = rotation;
			if (Vector3.Distance (transform.position, target.Value) < 1) {
				target = null;
				if (callback != null)
					callback.OnCompleteMove ();
			}
		}
		
	}

	public void MoveTo (Vector3 vec3)
	{
		vec3.x -= delta.x;
		vec3.y -= delta.y;
		vec3.z -= delta.z;
		target = vec3;
	}

	public void SetDelta (Vector3 vec3)
	{
		delta = vec3;
		delta.x -= transform.position.x;
		delta.y -= transform.position.y;
		delta.z -= transform.position.z;
	}

	public void SetDamage (int damage, TowerScript.TowerType type)
	{
		//TODO: add type
		health -= damage;
		if (health < 0) {
			for (int i = 0; i < towers.Count; i++) {
				towers[i].DestroyUnit (this);
			}
			Destroy (gameObject);
			if (callback != null)
				callback.OnDestroy ();
		}
	}

	public void SetMoveStatusListener (MoveStatus listener)
	{
		callback = listener;
	}

	public interface MoveStatus
	{
		void OnCompleteMove ();

		void OnDestroy ();
	}
}
