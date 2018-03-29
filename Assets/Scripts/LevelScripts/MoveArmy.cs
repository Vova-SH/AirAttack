using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveArmy : MonoBehaviour, UnitScript.MoveStatus
{
	public WayPoints[] wayPoints;
	public UnitScript[] units;
	public int activeUnits;

	private int completeMove = 0, currentPoint = 0;
	private Coroutine onWaitCoroutine = null;

	void Start ()
	{
		activeUnits = units.Length;
		foreach (UnitScript unit in units) {
			unit.SetMoveStatusListener (this);
			unit.SetDelta (wayPoints [0].transform.position);
		}
		onWaitCoroutine = StartCoroutine ("Wait");
	}

	void OnDrawGizmos ()
	{
		if (wayPoints.Length < 2)
			return;
		
		Gizmos.color = Color.white;
		for (int i = 1; i < wayPoints.Length; i++) {
			if (wayPoints [i - 1].transform == null || wayPoints [i].transform == null)
				return;
			Gizmos.DrawLine (wayPoints [i - 1].transform.position, wayPoints [i].transform.position);
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

	void UnitScript.MoveStatus.OnDestroy ()
	{
		activeUnits--;

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
				unit.MoveTo (wayPoints [currentPoint].transform.position);
			}
		} else {
		//TODO: add finish level
		}
	}

}
