using UnityEngine;
using System.Collections;

public static class SplineMovable
{
	public static void UpdateGameObject(Transform transform, Spline spline, WrapMode wMode, float passedTime)
	{
		transform.position = spline.GetPositionOnSpline( SplineMovable.WrapValue( passedTime, 0f, 1f, wMode ) );
		transform.rotation = spline.GetOrientationOnSpline( SplineMovable.WrapValue( passedTime, 0f, 1f, wMode ) );
	}

	public static float WrapValue( float v, float start, float end, WrapMode wMode )
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
