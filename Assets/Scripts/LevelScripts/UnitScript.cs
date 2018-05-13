﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitScript : MonoBehaviour
{
	public AudioSource death;
	private List<TowerScript> towers = new List<TowerScript> ();
	public int health = 100;
	public int armor = 0;
	public float speed = 5;

	private float target = -1;
	private Vector3 delta;
	private MoveStatus callback = null;

	private Spline spline = null;
	private unsafe WrapMode* wrapMode = null;
	private unsafe float* progress = null, offset = null;
	private float deltaProgress = 0, endProgress = 0;

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

	unsafe void Update ()
	{
		if (target > 0) {
			var passed = ((*progress) - deltaProgress) * speed + *offset;
			SplineMovable.UpdateGameObject (transform, delta, spline, *wrapMode, passed);
			transform.position -= delta;

			if (target <= passed) {
				target = -1;
				endProgress = *progress;
				if (callback != null)
					callback.OnCompleteMove ();
			}
		}
		
	}

	public unsafe void MoveTo (float progress)
	{
		deltaProgress += (*this.progress) - endProgress;
		target = progress;
	}

	public void SetDelta (Vector3 vec3)
	{
		delta = vec3;
		delta.x -= transform.position.x;
		delta.y -= transform.position.y;
		delta.z -= transform.position.z;
	}

	public unsafe void SetProgress (float* progress)
	{
		this.progress = progress;
	}

	public unsafe void SetOffset (float* offset)
	{
		this.offset = offset;
	}

	public void SetSpline (Spline spline)
	{
		this.spline = spline;
	}

	public unsafe void SetWrapMode (WrapMode* mode)
	{
		wrapMode = mode;
	}

	public void SetDamage (int damage)
	{
		//TODO: add type
		health -= damage;
		if (health < 1) {
			for (int i = 0; i < towers.Count; i++) {
				towers [i].DestroyUnit (this);
			}
			death.Play ();
			Destroy (gameObject);
			if (callback != null)
				callback.OnDestroy ();
		}
	}

	unsafe void OnDestroy()
	{
		spline = null;
		progress = null;
		wrapMode = null;
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
