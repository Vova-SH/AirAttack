using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WayPoint {
	[Range(0, 1)]
	public float progress;
	public int waitTime = 0;
}
