using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPosition : MonoBehaviour {
	public void SetPositionY(Transform anchor)
    {
            var rot = anchor.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(0, rot.y, 0);
    }
}
