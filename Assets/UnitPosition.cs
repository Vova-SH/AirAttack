using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitPosition : MonoBehaviour {
	public Transform watchUnit;
	// Update is called once per frame
	void Update () {
		Vector3 vec = watchUnit.position-transform.transform.position;
		//float z = Quaternion.FromToRotation(transform.position,watchUnit.position).eulerAngles.z;
		//vec.z=transform.position.z;
		transform.LookAt(watchUnit);
	}
}
