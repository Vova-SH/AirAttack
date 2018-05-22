using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MenuElementChanger : MonoBehaviour
{
	public bool ignoreTransition = false;
	public MenuElementChanger[] onActivateItems;
	public UnityEvent onEnable;
	public UnityEvent onDisable;

	void Start ()
	{
		if (ignoreTransition)
			return;
		EventTrigger eventTrigger = GetComponent<EventTrigger> ();
		if (eventTrigger == null)
			eventTrigger = gameObject.AddComponent<EventTrigger> ();
		EventTrigger.Entry pointerClick = new EventTrigger.Entry ();
		pointerClick.eventID = EventTriggerType.PointerClick;
		pointerClick.callback.AddListener ((eventData) => {
			GameObject.Find ("System").GetComponent<TransitionManagerMenu> ()
				.changePosition (GetComponent<MenuElementChanger> ());
			onDisable.Invoke ();
			foreach (MenuElementChanger item in onActivateItems) {
				item.enable ();
			}
		});
		eventTrigger.triggers.Add (pointerClick);
	}

	public void enable ()
	{
		onEnable.Invoke ();
	}

	public void disable ()
	{
		onDisable.Invoke ();
	}
}
