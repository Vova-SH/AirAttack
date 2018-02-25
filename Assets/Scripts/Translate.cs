using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translate : MonoBehaviour {

	public Transform fromTransform;

	public void setTranform(Transform toTransform)
	{
		fromTransform.SetPositionAndRotation(toTransform.position, toTransform.rotation);
	}
}
