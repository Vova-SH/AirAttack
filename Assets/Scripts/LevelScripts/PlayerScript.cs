using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour, MoveArmy.State
{

	public MoveArmy moveArmy;
	public PlayerWayPoint[] wayPoints;
	public Transform parentDefaultWaitPoint;
	public Transform[] defaultWaitPoints;
	public Transform forward;
	public int speed = 10;
	public int angularVelocity = 1;

	private int pointNum = -1, armyPointNum = -1;
	private Vector3? point = null;
	private Transform waitPointParent;
	private int waitPointNum = -1;

	void Start ()
	{
		moveArmy.setStateListener (this);
		waitPointParent = parentDefaultWaitPoint.parent;
		if (moveArmy.startWaitTime == 0 && wayPoints.Length > 0)
			point = wayPoints [0].transform.position;
		else if (defaultWaitPoints.Length > 0) {
			waitPointNum = 0;
			point = defaultWaitPoints [0].position;
			waitPointParent = parentDefaultWaitPoint.parent;
			parentDefaultWaitPoint.parent = null;
			moveArmy.FixWait = true;
		}
	}

	void Update ()
	{
		if (point == null) {
			if (waitPointNum == -1) {
				if (armyPointNum > pointNum && wayPoints.Length > pointNum) {
					moveArmy.FixWait = false;
					parentDefaultWaitPoint.parent = waitPointParent;
					point = wayPoints [pointNum + 1].transform.position;
				} else if (defaultWaitPoints.Length > 0) {
					waitPointNum = 0;
					moveArmy.FixWait = true;
					if (pointNum < 0 || wayPoints [pointNum].waitPoints.Length < 1) {
						point = defaultWaitPoints [0].position;
						parentDefaultWaitPoint.parent = null;
					} else {
						point = wayPoints [pointNum].waitPoints [0].position;
					}
				}
			} else if ((pointNum < 0 || wayPoints [pointNum].waitPoints.Length < 1) ? 
				defaultWaitPoints.Length > waitPointNum : wayPoints [pointNum].waitPoints.Length > waitPointNum) {
				point =(pointNum < 0 || wayPoints [pointNum].waitPoints.Length < 1) ? defaultWaitPoints [waitPointNum].position : wayPoints [pointNum].waitPoints [waitPointNum].position;
			} else {
				point = pointNum == -1 ? parentDefaultWaitPoint.position : wayPoints [pointNum].transform.position;
				waitPointNum = -2;
			}
		} else {
			transform.position = Vector3.MoveTowards (transform.position, forward.position, speed * Time.deltaTime);
			Quaternion relativePos = Quaternion.LookRotation (point.Value - transform.position);
			Quaternion rotation = Quaternion.RotateTowards (transform.rotation, relativePos, angularVelocity * Time.deltaTime);
			transform.rotation = rotation;
			if (Vector3.Distance (transform.position, point.Value) < 1) {
				point = null;
				if (waitPointNum == -1)
					pointNum++;
				else
					waitPointNum++;
			}
		}
	}

	void MoveArmy.State.AllUnitComplete (int point)
	{
		armyPointNum = point;
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
