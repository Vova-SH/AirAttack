using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenu : MonoBehaviour {
	public void SetPlayerPosition(Transform transform){
		Transform myTranform = GetComponent<Transform> ();
		myTranform.SetPositionAndRotation (transform.position, myTranform.rotation);
	}
}
