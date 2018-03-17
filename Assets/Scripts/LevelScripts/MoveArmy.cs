using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveArmy : MonoBehaviour, UnitScript.MoveStatus
{
	public WayPoints[] wayPoints;
	public UnitScript[] units;

	private int completeMove = 0, currentPoint = 0;
	private Coroutine onWaitCoroutine = null;

	void Start ()
	{
		foreach (UnitScript unit in units) {
			unit.SetMoveStatusListener (this);
			unit.SetDelta (wayPoints [0].gameObject.transform.position);
		}
		onWaitCoroutine = StartCoroutine ("Wait");
	}

	void OnDrawGizmos ()
	{
		if (wayPoints.Length < 2)
			return;
		
		Gizmos.color = Color.white;
		for (int i = 1; i < wayPoints.Length; i++) {
			if (wayPoints [i - 1].gameObject == null || wayPoints [i].gameObject == null)
				return;
			Gizmos.DrawLine (wayPoints [i - 1].gameObject.transform.position, wayPoints [i].gameObject.transform.position);
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
		if (completeMove == units.Length) 
		{
			onWaitCoroutine = StartCoroutine ("Wait");
			//TODO: add stop!
		}
	}

	IEnumerator Wait()
	{
		yield return new WaitForSeconds(wayPoints [currentPoint].waitTime);
		currentPoint++;
		if (currentPoint < wayPoints.Length) {
			completeMove = 0;
			foreach (UnitScript unit in units) {
				unit.MoveTo (wayPoints [currentPoint].gameObject.transform.position);
			}
		} else {
		//TODO: add finish level
		}
	}

}
