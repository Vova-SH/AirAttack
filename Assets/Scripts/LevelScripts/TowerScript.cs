using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerScript : MonoBehaviour
{
	[Header("Gun Settings")]
	public Transform head, gun;
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
		var rotate = angularVelocity * Time.deltaTime;
		if (units.Count != 0 && health > 0) {
			//RotateOY(units [units.Count - 1].transform.position, rotate);
			//RotateOX(units [units.Count - 1].transform.position, rotate);

			
			Vector3 target = units [0].transform.position - gun.transform.position;
			target.y = gun.transform.forward.y;
			float angle = Vector3.SignedAngle (target, gun.transform.forward, Vector3.up);
			if (Mathf.Abs (angle) > rotate) {
				var vector = angle < 0 ? Vector3.up : -Vector3.up;
				gun.transform.Rotate (vector, rotate, Space.Self);
			}
		} else {
			float angle = gun.rotation.eulerAngles.y % 360;
			if (Mathf.Abs (angle) > rotate) {
				var vector = angle > 0 && angle < 180 ? -Vector3.up : Vector3.up;
				gun.Rotate (vector, angularVelocity * Time.deltaTime, Space.Self);
			} else if (health <= 0) {
				gun.rotation = Quaternion.identity;
				gunAnimator.Play (disableGun);
			}
		}
	}

	void RotateOY(Vector3 target, float rotate)
	{
		if(gun==null) return;
		Vector3 toTarget = target - gun.position;
			toTarget.y = gun.forward.y;
			float angle = Vector3.SignedAngle (target, gun.forward, Vector3.up);
			if (Mathf.Abs (angle) > rotate) {
				var vector = angle < 0 ? Vector3.up : -Vector3.up;
				gun.Rotate (vector, rotate, Space.Self);
			}
	}
	void RotateOX(Vector3 target, float rotate)
	{
		if(head==null) return;
		Vector3 toTarget = target - head.position;
			toTarget.x = head.forward.x;
			float angle = Vector3.SignedAngle (target, head.forward, Vector3.left);
			if (Mathf.Abs (angle) > rotate) {
				var vector = angle < 0 ? Vector3.left : -Vector3.left;
				head.Rotate (vector, rotate, Space.Self);
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
			progressBar.transform.parent.gameObject.SetActive (false);
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
				go.GetComponent<BulletScript> ().Target = units [0];
			}
		}
	}

}
