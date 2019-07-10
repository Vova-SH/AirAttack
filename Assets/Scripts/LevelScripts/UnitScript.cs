using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitScript : MonoBehaviour
{
	public AudioSource death;
	private List<TowerScript> towers = new List<TowerScript> ();
	public ParticleSystem damage;
	public int health = 100;
	public int armor = 0;
	public float speed = 5;

	[Header("UI Indicator")]
	public RectTransform layerForIndicator;
	public UnitPosition indicator;
	public Color colorIndicator;
	private float target = -1;
	private Vector3 delta;
	private MoveStatus callback = null;

	private Spline spline = null;
	private unsafe WrapMode* wrapMode = null;
	private unsafe float* progress = null, offset = null;
	private float deltaProgress = 0, endProgress = 0;
	private int totalHealth;

	void Awake()
	{
		totalHealth = health;
	}

	void Start ()
	{
		indicator.GetComponent<Image> ().color = colorIndicator;
		indicator.watchUnit = gameObject;
		GameObject.Instantiate(indicator, layerForIndicator.transform.position, Quaternion.identity, layerForIndicator.transform);
		//TODO: add trigger, when shoot collide
	}

	void OnTriggerEnter (Collider other)
	{
		TowerScript tower = other.gameObject.GetComponent<TowerScript> ();
		if (tower != null) {
			towers.Add (tower);
		}
	}

	void OnTriggerExit (Collider other)
	{
		TowerScript tower = other.gameObject.GetComponent<TowerScript> ();
		if (tower != null) {
			towers.Remove (tower);
		}
	}

	unsafe void Update ()
	{
		if (target > 0) {
			var passed = ((*progress) - deltaProgress) * speed + *offset;
			SplineMovable.UpdateGameObject (transform, spline, *wrapMode, passed);
			transform.Translate( -delta);

			if (target <= passed) {
				target = -1;
				endProgress = *progress;
				if (callback != null)
					callback.OnCompleteMove ();
			}
		}
		
	}

	public unsafe void MoveTo (float progress)
	{
		deltaProgress += (*this.progress) - endProgress;
		target = progress;
	}

	public void SetDelta (Vector3 vec3)
	{
		delta = vec3;
		delta.x -= transform.position.x;
		delta.y -= transform.position.y;
		delta.z -= transform.position.z;
	}

	public unsafe void SetProgress (float* progress)
	{
		this.progress = progress;
	}

	public unsafe void SetOffset (float* offset)
	{
		this.offset = offset;
	}

	public void SetSpline (Spline spline)
	{
		this.spline = spline;
	}

	public unsafe void SetWrapMode (WrapMode* mode)
	{
		wrapMode = mode;
	}

	public void SetDamage (int damage)
	{
		//TODO: add type
		health -= damage;
		var emission = this.damage.emission;
        emission.rateOverTime = (totalHealth - health)/((float)totalHealth)*100;
		//this.damage.emission.rateOverTime.constant = (totalHealth - health)/((float)totalHealth)*100;
	
		if (health < 1) {
			for (int i = 0; i < towers.Count; i++) {
				towers [i].DestroyUnit (this);
			}
			if(death!=null) death.Play ();
			Destroy (gameObject);
			if (callback != null)
				callback.OnDestroy ();
		}
	}

	unsafe void OnDestroy()
	{
		spline = null;
		progress = null;
		wrapMode = null;
	}

	public void SetMoveStatusListener (MoveStatus listener)
	{
		callback = listener;
	}

	public interface MoveStatus
	{
		void OnCompleteMove ();

		void OnDestroy ();
	}
}
