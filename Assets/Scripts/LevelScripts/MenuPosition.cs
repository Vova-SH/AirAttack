using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPosition : MonoBehaviour {
	public void SetPositionY(Transform anchor)
    {
            Vector3 target = anchor.position - transform.position;
			target.y = transform.forward.y;
			var angle = Vector3.SignedAngle (target, transform.forward, Vector3.up);
			transform.Rotate (-Vector3.up, angle, Space.World);
    }
}
