using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
	
	public PlayerWayPoint[] wayPoints;
	public Transform[] defaultWaitPoints;
	public int speed = 10;
	Quaternion quaternion;
	
	void Start () {
		quaternion = new Quaternion();
	}

	void Update () {
		//write correct point
		quaternion.SetFromToRotation (transform.position, wayPoints[2].transform.position);
		transform.position = Vector3.Lerp (transform.position, wayPoints [2].transform.position, speed * Time.deltaTime);
		transform.rotation = quaternion * transform.rotation;
	}

	void OnDrawGizmos ()
	{
		if (wayPoints.Length < 2)
			return;

		Gizmos.color = Color.green;
		for (int i = 1; i < wayPoints.Length; i++) {
			if (wayPoints [i - 1].transform == null || wayPoints [i].transform == null)
				return;
			Gizmos.DrawLine (wayPoints [i - 1].transform.position, wayPoints [i].transform.position);
		}
	}
}
