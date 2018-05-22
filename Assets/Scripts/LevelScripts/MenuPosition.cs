using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPosition : MonoBehaviour {
	public void SetPositionY(Transform anchor)
    {
            var rot = anchor.rotation.eulerAngles;
            Debug.Log(rot);
			transform.rotation = Quaternion.Euler(0, rot.y, 0);
    }
}
