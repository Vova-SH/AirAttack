using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveArmy : MonoBehaviour, UnitScript.MoveStatus
{
	public WayPoint[] wayPoints;
	public UnitScript[] units;
	public int startWaitTime = 0;
	public float offset = 0;
	public Spline spline;
	public WrapMode wrapMode = WrapMode.Clamp;

	private int activeUnits;
	private int completeMove = 0, currentPoint = 0;
	private Coroutine onWaitCoroutine = null;
	private TransitionLevelManager manager = null;


	public float progress = 0;

	unsafe void Start ()
	{
		manager = GameObject.FindObjectOfType<TransitionLevelManager> ();
		transform.position = spline.GetPositionOnSpline (SplineMovable.WrapValue (offset, 0f, 1f, wrapMode));
		activeUnits = units.Length;
		foreach (UnitScript unit in units) {
			unit.SetMoveStatusListener (this);
			unit.SetDelta (transform.position);
			unit.SetSpline(spline);
			fixed (float* link = &progress){
				unit.SetProgress (link);
			};

			fixed (float* link = &offset){
				unit.SetOffset (link);
			};

			fixed (WrapMode* link = &wrapMode){
				unit.SetWrapMode(link);
			};
		}
		onWaitCoroutine = StartCoroutine ("Wait");
	}

	void OnDrawGizmos ()
	{		
		Gizmos.color = new Color (1, 0, 0, 1F);
		Gizmos.DrawSphere (spline.GetPositionOnSpline(SplineMovable.WrapValue( offset, 0f, 1f, wrapMode )), 1f);
	}

	void FixedUpdate( ) 
	{
		progress += Time.deltaTime;
		spline.UpdateSplineNodes();
	}

	[ContextMenu ("Add all Units in this object")]
	public void AddAllUnits ()
	{
		units = GameObject.FindObjectsOfType<UnitScript> ();
	}

	[ContextMenu ("Set army to start")]
	public void SetToProgress ()
	{
		transform.position = spline.GetPositionOnSpline (SplineMovable.WrapValue (offset, 0f, 1f, wrapMode));
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
				completeMove = 0;
				foreach (UnitScript unit in units) {
					if(unit!=null)
					unit.MoveTo (wayPoints [currentPoint].progress);
				}
				onWaitCoroutine = null;
				currentPoint++;
				if (currentPoint < wayPoints.Length)
					startWaitTime = wayPoints [currentPoint].waitTime;
		} else {
			if (manager != null)
				manager.showMenu (activeUnits);
		}
	}
}
