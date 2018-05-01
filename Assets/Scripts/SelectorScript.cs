using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorScript : MonoBehaviour {

	public void SetItem(Transform obj)
	{
		transform.rotation = obj.rotation;
	}
}
