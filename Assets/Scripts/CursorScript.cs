using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorScript : MonoBehaviour {

        [SerializeField]
        protected LayerMask blockingMask;

	void Start () {
		
	}

	void Update () {
		Debug.DrawRay (transform.position, transform.forward * 100);
	}
}
