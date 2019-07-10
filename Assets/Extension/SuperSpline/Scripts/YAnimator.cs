using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class YAnimator : MonoBehaviour
{
	public float passedTime = 0f;
	public float yOffset = 0f;
	public float speed = 0f;
	
	private Vector3 normalPosition;
	
	void Start( )
	{
		normalPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update( ) 
	{
		passedTime += Time.deltaTime * speed;
		
		transform.position = normalPosition + Vector3.up * yOffset * Mathf.Sin( passedTime );;
		
	}
}
