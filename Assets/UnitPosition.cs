using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class UnitPosition : MonoBehaviour
{
    public GameObject watchUnit;
    // Update is called once per frame
    void Update()
    {
		Vector3 vec = watchUnit.transform.position-transform.position;
		transform.rotation = Quaternion.LookRotation(transform.forward,vec);
		float y=Quaternion.LookRotation(transform.up,vec).eulerAngles.y;
		float x = Quaternion.LookRotation(transform.right,vec).eulerAngles.x;
		GetComponent<Image>().enabled=!watchUnit.GetComponentInChildren<Renderer>().isVisible;
    }

}
