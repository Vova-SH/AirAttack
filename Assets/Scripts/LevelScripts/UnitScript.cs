using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
	public int health = 100;
	public int armor = 0;
	public int speed = 5;


	public Resistable[] resistance;

	private Vector3? target = null;
	private Vector3 delta;
	private MoveStatus callback = null;

	void Start ()
	{
		//TODO: add trigger, when shoot collide
	}

	void Update ()
	{
		if (target != null) {
			transform.position = Vector3.MoveTowards (transform.position, target.Value, speed * Time.deltaTime);
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

	public bool SetDamage (int damage, TowerScript.TowerType type)
	{
		//TODO: add type
		health -= damage;
		if (health < 0) {
			Destroy (gameObject);
			if (callback != null)
				callback.OnDestroy ();
		}
		return health < 0;
	}

	public void SetMoveStatusListener (MoveStatus listener)
	{
		callback = listener;
	}

	public interface MoveStatus
	{
		void OnCompleteMove ();
		void OnDestroy();
	}
}
