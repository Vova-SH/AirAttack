using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuLevelItem : MonoBehaviour
{
	public Text textView;

	public string onSelectText;

	void Start ()
	{
		EventTrigger eventTrigger = GetComponent<EventTrigger> ();
		if (eventTrigger == null)
			eventTrigger = gameObject.AddComponent<EventTrigger> ();
		EventTrigger.Entry pointerEnter = new EventTrigger.Entry ();
		pointerEnter.eventID = EventTriggerType.PointerEnter;
		pointerEnter.callback.AddListener ((eventData) => {
			textView.text = onSelectText;
		});

		EventTrigger.Entry pointerExit = new EventTrigger.Entry ();
		pointerExit.eventID = EventTriggerType.PointerExit;
		pointerExit.callback.AddListener ((eventData) => {
			textView.text = null;
		});
		eventTrigger.triggers.Add (pointerEnter);
		eventTrigger.triggers.Add (pointerExit);
	}
}
