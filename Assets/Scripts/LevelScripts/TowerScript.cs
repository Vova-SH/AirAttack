using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerScript : MonoBehaviour
{
	[Header("Gun Settings")]
	public GameObject gun;
	public Transform shootStart;
	public AudioSource shootAudio;
	public ParticleSystem particleGun;
	public Animator gunAnimator;
	public string disableGun;
	public float angularVelocity = 2f;

	[Header("State Settings")]
	public int health = 30;
	public AudioSource deathAudio;
	public Image progressBar;

	[Header("Shooting Settings")]
	public BulletScript bullet;
	public int radius = 10;
	public float timeoutSeconds = 1.5f;

	private List<UnitScript> units = new List<UnitScript> ();
	private SphereCollider sphereCollider;
	private Coroutine shoot = null;
	private float multiplyProgress = 0;

	void Start ()
	{
		multiplyProgress = 1f / health;
		bullet.player = GameObject.FindWithTag ("Player");
		sphereCollider = gameObject.AddComponent<SphereCollider> ();
		sphereCollider.radius = radius;
		sphereCollider.isTrigger = true;
	}

	void OnTriggerEnter (Collider other)
	{
		UnitScript unit = other.gameObject.GetComponent<UnitScript> ();
		if (unit != null) {
			units.Add (unit);
			if (shoot == null)
				shoot = StartCoroutine ("Shoot");
		}
	}

	void OnTriggerExit (Collider other)
	{
		UnitScript unit = other.gameObject.GetComponent<UnitScript> ();
		if (unit != null) {
			units.Remove (unit);
			if (units.Count == 0 && shoot != null)
				StopCoroutine (shoot);
		}
	}

	void Update ()
	{
		float angle;
		var rotate = angularVelocity * Time.deltaTime;
		if (units.Count != 0 && health > 0) {
			Vector3 target = units [units.Count - 1].transform.position - gun.transform.position;
			target.y = gun.transform.forward.y;
			angle = Vector3.SignedAngle (target, gun.transform.forward, Vector3.up);
			if (Mathf.Abs (angle) > rotate) {
				var vector = angle < 0 ? Vector3.up : -Vector3.up;
				gun.transform.Rotate (vector, rotate, Space.Self);
			}
			/*
			var lookPos = units [units.Count - 1].transform.position - gun.transform.position;
			lookPos.y = 0;
			var rotation = Quaternion.LookRotation (lookPos);
			gun.transform.rotation = Quaternion.Slerp (gun.transform.rotation, rotation, Time.deltaTime * angularVelocity);*/
		} else {
			angle = gun.transform.rotation.eulerAngles.y % 360;
			if (Mathf.Abs (angle) > rotate) {
				var vector = angle > 0 && angle < 180 ? -Vector3.up : Vector3.up;
				gun.transform.Rotate (vector, angularVelocity * Time.deltaTime, Space.Self);
			} else if (health <= 0) {
				gun.transform.rotation = Quaternion.identity;
				gunAnimator.Play (disableGun);
			}
		}
	}

	public void DestroyUnit (UnitScript unit)
	{
		units.Remove (unit);
	}

	public void SetDamage (int damage)
	{
		int cur = health;
		health -= damage;
		progressBar.fillAmount = health * multiplyProgress;
		if (cur > 0 && health < 1) {
			progressBar.gameObject.SetActive (false);
			deathAudio.Play ();
		}
		//change skin or add particle
	}

	void OnDrawGizmosSelected ()
	{
		Gizmos.color = new Color (1, 1, 0, 0.5F);
		Gizmos.DrawSphere (transform.position, radius);
	}

	/*
	void OnDrawGizmos() {
			Gizmos.color = new Color (1, 1, 0, 0.75F);
			Gizmos.DrawSphere (transform.position, radius);
	}
	*/
	IEnumerator Shoot ()
	{
		while (units.Count > 0 && health>0) {
			yield return new WaitForSeconds (timeoutSeconds);
			particleGun.Play ();
			shootAudio.Play ();
			if (units.Count > 0 && health > 0) {
				GameObject go = Instantiate (bullet.gameObject, shootStart.position, Quaternion.identity) as GameObject;
				go.GetComponent<BulletScript> ().Target = units [units.Count - 1];
			}
			yield return new WaitForSeconds (0.1f);
			particleGun.Stop ();
		}
	}

}
