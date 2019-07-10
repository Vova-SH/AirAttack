using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OVREventTrigger : MonoBehaviour {
	
	public UnityEvent OneGetUp;
	void Awake(){
		EventTrigger eventTrigger = GetComponent<EventTrigger> ();
		if (eventTrigger == null)
			eventTrigger = gameObject.AddComponent<EventTrigger> ();
		EventTrigger.Entry pointerClick = new EventTrigger.Entry ();
		pointerClick.eventID = EventTriggerType.PointerClick;
		pointerClick.callback.AddListener ((eventData) => {
			if(OVRInput.GetUp(OVRInput.Button.One)) OneGetUp.Invoke();
		});
		eventTrigger.triggers.Add (pointerClick);
	}
}
