using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
	public PlayerBulletScript bullet;
	public Transform[] wayPoints;
	public Transform ovrCamera;
	public int speed = 10;
	public float reloadTime = 0.1f;
	public int angularVelocity = 1;

	private bool isReloaded = true, activeShoot = true;
	private int pointNum = 0;
	private Vector3? point = null;

	void Start ()
	{
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
			Quaternion relativePos = Quaternion.LookRotation (point.Value - transform.position);
			Quaternion rotation = Quaternion.RotateTowards (transform.rotation, relativePos, angularVelocity * Time.deltaTime);
			transform.rotation = rotation;
			if (Vector3.Distance (transform.position, point.Value) < 1) {
				point = null;
				pointNum++;
			}
		}

		if (Input.GetMouseButtonDown (0) && isReloaded && activeShoot) {
			isReloaded = false;
			StartCoroutine (Reload ());
			Instantiate (bullet.gameObject, ovrCamera.position, ovrCamera.rotation);
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
		yield return new WaitForSeconds (reloadTime);
		isReloaded = true;
	}
}
