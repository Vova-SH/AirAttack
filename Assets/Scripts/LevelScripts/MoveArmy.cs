using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveArmy : MonoBehaviour, UnitScript.MoveStatus
{
	public WayPoint[] wayPoints;
	public UnitScript[] units;
	public int startWaitTime = 0;

	private int activeUnits;
	private int completeMove = 0, currentPoint = 0;
	private Coroutine onWaitCoroutine = null;
	private State callbackState = null;
	private bool waitComplete = false;
	private bool fixWait = false;
	public bool FixWait
	{
		set
		{
			if (!value && waitComplete) {
				StartCoroutine ("Wait");
				waitComplete = false;
			}
			fixWait = value;
		}

		get{ return fixWait; }
	}

	void Start ()
	{
		activeUnits = units.Length;
		foreach (UnitScript unit in units) {
			unit.SetMoveStatusListener (this);
			unit.SetDelta (transform.position);
		}
		onWaitCoroutine = StartCoroutine ("Wait");
	}

	void OnDrawGizmos ()
	{
		if (wayPoints.Length < 1)
			return;
		
		Gizmos.color = Color.white;
		Vector3 lastPos = transform.position;
		for (int i = 0; i < wayPoints.Length; i++) {
			if (wayPoints [i].transform == null)
				continue;
			Gizmos.DrawLine (lastPos, wayPoints [i].transform.position);
			lastPos = wayPoints [i].transform.position;
		}
	}

	[ContextMenu ("Add all Units in this object")]
	public void AddAllUnits ()
	{
		units = GameObject.FindObjectsOfType<UnitScript> ();
	}

	void UnitScript.MoveStatus.OnCompleteMove ()
	{
		completeMove++;
		if (completeMove == activeUnits) {
			onWaitCoroutine = StartCoroutine ("Wait");
		}
	}

	void UnitScript.MoveStatus.OnDestroy ()
	{
		activeUnits--;
		if (onWaitCoroutine == null && completeMove == activeUnits) {
			onWaitCoroutine = StartCoroutine ("Wait");
		} else if (onWaitCoroutine != null && activeUnits < 1) {
			StopCoroutine ("Wait");
			//TODO: add failLevel!
		}
	}

	IEnumerator Wait ()
	{
		yield return new WaitForSeconds (startWaitTime);
		if (currentPoint < wayPoints.Length) {
			if (callbackState != null)
				callbackState.AllUnitComplete (currentPoint);
			if (fixWait == true) {
				startWaitTime = 0;
				waitComplete = true;
			} else {
				completeMove = 0;
				foreach (UnitScript unit in units) {
					unit.MoveTo (wayPoints [currentPoint].transform.position);
				}
				onWaitCoroutine = null;
				currentPoint++;
				if (currentPoint < wayPoints.Length)
					startWaitTime = wayPoints [currentPoint].waitTime;
			}
		} else {
			//TODO: add finish level
		}
	}

	public void setStateListener (State state)
	{
		callbackState = state;
	}

	public interface State
	{
		void AllUnitComplete (int point);
	}
}
