﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
	[Header("Panel Initialize")]
	public GameObject bulletsIndicators;
	public Image bulletIndicatorPrefab;
	public GameObject reloadIndicator;

	[Header("Move Settings")]
	public int speed = 10;
	public int angularVelocity = 1;
	public Transform[] wayPoints;

	[Header("Shoot Settings")]
	public GameObject bulletPrefab;
	public float reloadShootTime = 0.1f;
	public Transform[] shootStart;

	[Header("Bullet Settings")]
	public int bulletCount = 10;
	public float reloadBulletTime = 2.0f;

	[Header("Other Initialize")]
	public Transform ovrCamera;

	private bool isReloaded = true, activeShoot = true;
	private int pointNum = 0;
	private int bulletCout;
	private Image[] bullets;
	private Vector3? point = null;

	void Start ()
	{
		bullets = new Image[bulletCount];
		bulletCout = bulletCount;
		for (int i = 0; i < bulletCount; i++) 
		{
			bullets [i] = Instantiate (bulletIndicatorPrefab, bulletsIndicators.transform);
			bullets [i].color = Color.white;
		}
		if(wayPoints.Length>0)
		point = wayPoints [0].position;
	}

	void Update ()
	{
		if (point == null) {
			if (pointNum < wayPoints.Length)
				point = wayPoints [pointNum].position;
			else
				point = null;
		} else {
			transform.position += transform.forward * speed * Time.deltaTime;


			Transform parentOvr = ovrCamera.parent;
			ovrCamera.parent = null;

			Quaternion relativePos = Quaternion.LookRotation (point.Value - transform.position);
			Quaternion rotation = Quaternion.RotateTowards (transform.rotation, relativePos, angularVelocity * Time.deltaTime);
			transform.rotation = rotation;

			ovrCamera.parent = parentOvr;
			if (Vector3.Distance (transform.position, point.Value) < 1) {
				point = null;
				pointNum++;
			}
		}
		if (isReloaded) {
			if ((Input.GetKey (KeyCode.R) || OVRInput.Get (OVRInput.Button.DpadLeft)) && bulletCout < bulletCount) {
				isReloaded = false;
				StartCoroutine (ReloadBow ());
			} else if ((Input.GetMouseButtonDown (0) || OVRInput.Get (OVRInput.Button.One)) && activeShoot && bulletCout > 0) {
				isReloaded = false;
				StartCoroutine (Reload ());
				GetComponent<AudioSource> ().Play ();
				bulletCout--;
				bullets [bulletCout].color = Color.gray;
				for (int i = 0; i < shootStart.Length; i++)
					Instantiate (bulletPrefab.gameObject, shootStart [i].position, shootStart [i].rotation);
				if(bulletCout==0) reloadIndicator.SetActive(true);
			}
		}
	}

	[ContextMenu ("Add all Player points")]
	public void AddAllPoints ()
	{
		List<GameObject> objs = new List<GameObject> ();
		objs.AddRange (GameObject.FindGameObjectsWithTag ("Player point"));
		wayPoints = new Transform[objs.Count];
		for (int i = 0; i < objs.Count; i++) {
			string name = objs [i].name;
			name = name.Substring (name.IndexOf ("(") + 1);
			name = name.Remove (name.Length - 1);
			wayPoints [int.Parse (name) - 1] = objs [i].transform;
		}
	}

	public void setShoot(bool isShooting){
		activeShoot = isShooting;
	}

	void OnDrawGizmos ()
	{
		if (wayPoints.Length < 1)
			return;

		Gizmos.color = Color.green;
		Vector3 lastPos = transform.position;
		for (int i = 0; i < wayPoints.Length; i++) {
			if (wayPoints [i].transform == null)
				continue;
			Gizmos.DrawLine (lastPos, wayPoints [i].transform.position);
			lastPos = wayPoints [i].transform.position;
		}
	}

	IEnumerator Reload ()
	{
		yield return new WaitForSeconds (reloadShootTime);
		isReloaded = true;
	}

	IEnumerator ReloadBow ()
	{
		reloadIndicator.SetActive(false);
		Animator anim = bulletsIndicators.GetComponent<Animator> ();
		if (anim != null)
		{
			anim.SetTrigger ("Reload");
		}
		yield return new WaitForSeconds (reloadBulletTime);
		if (anim != null)
		{
			anim.SetTrigger ("ReloadComplete");
		}
		for (int i = 0; i < bulletCount; i++) 
		{
			bullets [i].color = Color.white;
		}
		bulletCout = bulletCount;
		isReloaded = true;
	}
}
