using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//This class applies gravity towards a spline to rigidbodies that this script is attached to
public class SplineAnimatorGravity : MonoBehaviour
{
	public Spline spline;
	
	public float gravityConstant = 9.81f;
	public int iterations = 5;
	
	void FixedUpdate( ) 
	{
		if( GetComponent<Rigidbody>() == null || spline == null )
			return;
		
		Vector3 force = spline.GetShortestConnection( GetComponent<Rigidbody>().position, iterations );
		
		//Calculate gravity force according to Newton's law of universal gravity
		GetComponent<Rigidbody>().AddForce( force * ( Mathf.Pow( force.magnitude, -3 ) * gravityConstant * GetComponent<Rigidbody>().mass) );
	}
}
