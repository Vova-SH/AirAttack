using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuLevelItem : MonoBehaviour
{
	public int levelNum;
	public Text textView;
	public Image imageView;
	public Color disableColor;
	public Sprite selectedSprite;
	public string onSelectText;
	public TransitionManagerMenu transition;

	private Sprite oftenSprite;

	void Start ()
	{
		oftenSprite = imageView.sprite;
		if (PrefsManager.CompleteLevel < levelNum) {
			imageView.sprite = selectedSprite;
			imageView.color = disableColor;
		} else {
			EventTrigger eventTrigger = GetComponent<EventTrigger> ();
			if (eventTrigger == null)
				eventTrigger = gameObject.AddComponent<EventTrigger> ();
			EventTrigger.Entry pointerEnter = new EventTrigger.Entry ();
			pointerEnter.eventID = EventTriggerType.PointerEnter;
			pointerEnter.callback.AddListener ((eventData) => {
				textView.text = onSelectText;
				imageView.sprite = selectedSprite;
			});

			EventTrigger.Entry pointerExit = new EventTrigger.Entry ();
			pointerExit.eventID = EventTriggerType.PointerExit;
			pointerExit.callback.AddListener ((eventData) => {
				imageView.sprite = oftenSprite;
			});

			EventTrigger.Entry pointerClick = new EventTrigger.Entry ();
			pointerClick.eventID = EventTriggerType.PointerClick;
			pointerClick.callback.AddListener ((eventData) => {
				transition.LoadScenes(levelNum);
			});
			eventTrigger.triggers.Add (pointerEnter);
			eventTrigger.triggers.Add (pointerExit);
			eventTrigger.triggers.Add (pointerClick);
		}
	}
}
