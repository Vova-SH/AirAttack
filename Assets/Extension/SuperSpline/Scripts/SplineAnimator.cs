using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//This class animates a gameobject along the spline at a specific speed.
public class SplineAnimator : MonoBehaviour
{
	public Spline spline;
	
	public float speed = 1f;
	public float offSet = 0f;
	public WrapMode wrapMode = WrapMode.Clamp;
	
	public float passedTime = 0f;
	
	void FixedUpdate( ) 
	{
		passedTime += Time.deltaTime * speed;
		
		//SplineSegment segment = spline.GetSplineSegment(WrapValue( passedTime + offSet, 0f, 1f, wrapMode ));
		
		transform.position = spline.GetPositionOnSpline( WrapValue( passedTime + offSet, 0f, 1f, wrapMode ) );
		transform.rotation = spline.GetOrientationOnSpline( WrapValue( passedTime + offSet, 0f, 1f, wrapMode ) );
		
		//segment.EndNode.nodeTransform.position.x+= 20;
		
		/*  Doesn't work, read only position from EndNode
		segment.EndNode.nodeTransform.position = new Vector3(segment.EndNode.nodeTransform.position.x += 20,
															segment.EndNode.nodeTransform.position.y,
															segment.EndNode.nodeTransform.position.z);*/
		
		// segment.EndNode;
		/*for(int i = 0; i < spline.splineNodesTransform.Length; i++)
		{
			if(spline.splineNodesTransform[i].Equals(segment.EndNode.nodeTransform))
			{
				spline.splineNodesTransform[i].transform = new Vector3(spline.splineNodesTransform[i].transform.position.x + 20,
																		spline.splineNodesTransform[i].transform.position.y,
																		spline.splineNodesTransform[i].transform.position.z);// .position.x += 20;
			}
		}*/
		
		spline.UpdateSplineNodes();
	}
	 
	public float WrapValue( float v, float start, float end, WrapMode wMode )
	{
		switch( wMode )
		{
		case WrapMode.Clamp:
		case WrapMode.ClampForever:
			return Mathf.Clamp( v, start, end );
		case WrapMode.Default:
		case WrapMode.Loop:
			return Mathf.Repeat( v, end - start ) + start;
		case WrapMode.PingPong:
			return Mathf.PingPong( v, end - start ) + start;
		default:
			return v;
		}
	}
}
