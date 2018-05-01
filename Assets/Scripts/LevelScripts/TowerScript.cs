﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerScript : MonoBehaviour
{
	public AudioSource shootAudio, deathAudio;
	public Animator gunAnimator;
	public string disableGun;
	public ParticleSystem particleGun;
	public GameObject gun;
	public Image progressBar;
	public int health = 30;
	public Transform shootStart;
	public BulletScript bullet;
	public TowerType towerType = TowerType.OFTEN;
	public float angularVelocity;
	public int radius = 10, damage = 5;
	public float timeoutSeconds = 1.5f;

	public enum TowerType
	{
		OFTEN,
		ARMORED,
		COLD
	}

	private List<UnitScript> units = new List<UnitScript> ();
	private SphereCollider sphereCollider;
	private Coroutine shoot = null;
	private float multiplyProgress = 0;

	void Start ()
	{
		multiplyProgress = 1f / health;
		bullet.player = GameObject.FindWithTag ("Player");
		sphereCollider = gameObject.AddComponent<SphereCollider> ();
		sphereCollider.radius = radius;
		sphereCollider.isTrigger = true;
	}

	void OnTriggerEnter (Collider other)
	{
		UnitScript unit = other.gameObject.GetComponent<UnitScript> ();
		if (unit != null) {
			units.Add (unit);
			if (shoot == null)
				shoot = StartCoroutine ("Shoot");
		}
	}

	void OnTriggerExit (Collider other)
	{
		UnitScript unit = other.gameObject.GetComponent<UnitScript> ();
		if (unit != null) {
			units.Remove (unit);
			if (units.Count == 0 && shoot != null)
				StopCoroutine (shoot);
		}
	}

	void Update ()
	{
		if (units.Count != 0 && health > 0) {
			var lookPos = units [units.Count - 1].transform.position - gun.transform.position;
			lookPos.y = 0;
			var rotation = Quaternion.LookRotation (lookPos);
			gun.transform.rotation = Quaternion.Slerp (gun.transform.rotation, rotation, Time.deltaTime * angularVelocity);
		} else if (gun.transform.rotation.eulerAngles.y >= 15) {
			var lookPos = gun.transform.position;
			lookPos.y = 0;
			var rotation = Quaternion.LookRotation (lookPos);
			gun.transform.rotation = Quaternion.Slerp (gun.transform.rotation, rotation, Time.deltaTime * angularVelocity);
		} else if (health <= 0) {
			gun.transform.rotation = Quaternion.identity;
			gunAnimator.Play (disableGun);
		}
	}

	public void DestroyUnit (UnitScript unit)
	{
		units.Remove (unit);
	}

	public void SetDamage (int damage)
	{
		health -= damage;
		progressBar.fillAmount = health * multiplyProgress;
		if (health < 1)
			deathAudio.Play ();
		//change skin or add particle
	}

	void OnDrawGizmosSelected ()
	{
		Gizmos.color = new Color (1, 1, 0, 0.5F);
		Gizmos.DrawSphere (transform.position, radius);
	}

	/*
	void OnDrawGizmos() {
			Gizmos.color = new Color (1, 1, 0, 0.75F);
			Gizmos.DrawSphere (transform.position, radius);
	}
	*/
	IEnumerator Shoot ()
	{
		while (units.Count > 0 && health>0) {
			yield return new WaitForSeconds (timeoutSeconds);
			particleGun.Play ();
			shootAudio.Play ();
			if (units.Count > 0 && health > 0) {
				GameObject go = Instantiate (bullet.gameObject, shootStart.position, Quaternion.identity) as GameObject;
				go.GetComponent<BulletScript> ().Target = units [units.Count - 1];
			}
			yield return new WaitForSeconds (0.1f);
			particleGun.Stop ();
		}
	}

}
