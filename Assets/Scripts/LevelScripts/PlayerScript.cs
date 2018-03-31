using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour, MoveArmy.State
{

	public MoveArmy moveArmy;
	public PlayerWayPoint[] wayPoints;
	public Transform[] defaultWaitPoints;
	public Transform forward;
	public int speed = 10;
	public int angularVelocity = 1;

	private int pointNum = -1, armyPointNum = -1;
	private Vector3? point = null;

	void Start ()
	{
		moveArmy.setStateListener (this);
		if (moveArmy.startWaitTime == 0 && wayPoints.Length > 0)
			point = wayPoints [0].transform.position;
		else if (defaultWaitPoints.Length > 0)
			point = defaultWaitPoints [0].transform.position;
		//add bool when start wait path
	}

	void Update ()
	{
		//write correct point
		if (point == null) {
			if (armyPointNum > pointNum && wayPoints.Length > pointNum) {
				point = wayPoints [pointNum].transform.position;
			}
			//add circle
		} else {
			transform.position = Vector3.MoveTowards (transform.position, forward.position, speed * Time.deltaTime);
			Quaternion relativePos = Quaternion.LookRotation(point.Value - transform.position);
			Quaternion rotation = Quaternion.RotateTowards(transform.rotation, relativePos, angularVelocity * Time.deltaTime);
			transform.rotation = rotation;
			if (Vector3.Distance (transform.position, point.Value) < 1) {
				point = null;
				pointNum++;
			}
		}
	}

	void MoveArmy.State.AllUnitComplete (int point)
	{
		armyPointNum = point;
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
