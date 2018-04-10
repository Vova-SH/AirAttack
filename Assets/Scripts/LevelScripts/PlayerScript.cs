using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{

	public Transform[] wayPoints;
	public Transform forward;
	public int speed = 10;
	public int angularVelocity = 1;

	private int pointNum = 0;
	private Vector3? point = null;

	void Start ()
	{
		point = wayPoints [0].position;
	}

	void Update ()
	{
		if (point == null) {
			if(pointNum<wayPoints.Length)
			point = wayPoints [pointNum].position;
		} else {
			transform.position = Vector3.MoveTowards (transform.position, forward.position, speed * Time.deltaTime);
			Quaternion relativePos = Quaternion.LookRotation (point.Value - transform.position);
			Quaternion rotation = Quaternion.RotateTowards (transform.rotation, relativePos, angularVelocity * Time.deltaTime);
			transform.rotation = rotation;
			if (Vector3.Distance (transform.position, point.Value) < 1) {
				point = null;
				pointNum++;
			}
		}
	}

	[ContextMenu ("Add all Player points")]
	public void AddAllPoints ()
	{
		List<GameObject> objs = new List<GameObject> ();
		objs.AddRange (GameObject.FindGameObjectsWithTag ("Player point"));
		wayPoints = new Transform[objs.Count];
		for(int i = 0; i<objs.Count; i++){
			string name = objs [i].name;
			name = name.Substring (name.IndexOf("(")+1);
			name = name.Remove (name.Length - 1);
			wayPoints [int.Parse (name)-1] = objs [i].transform;
		}
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
}
