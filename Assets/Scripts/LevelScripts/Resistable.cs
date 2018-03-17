using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Resistable {
	public string tagObject;
	[Range (0,2)]
	public float resist = 1f;
}
