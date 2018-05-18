using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Image))]
public class UnitPosition : MonoBehaviour
{
    public GameObject watchUnit;
    
    void Update()
    {
    if(watchUnit!=null){
		Vector3 vec = watchUnit.transform.position-transform.position;
		transform.rotation = Quaternion.LookRotation(transform.forward,vec);
		GetComponent<Image>().enabled=!watchUnit.GetComponentInChildren<Renderer>().isVisible;
    } else Destroy(gameObject);
    }

}
