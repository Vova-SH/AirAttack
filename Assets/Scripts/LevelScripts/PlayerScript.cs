using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
	public Transform ovrCamera;
	public PlayerBulletScript bullet;
	public GameObject bowsIndicators;
	public Image bowIndicatorPrefab;
	public Transform[] wayPoints;
	public Transform[] shootStart;
	public int speed = 10;
	public float reloadBowTime = 2.0f;
	public float reloadTime = 0.1f;
	public int bowCount = 10;
	public int angularVelocity = 1;

	private bool isReloaded = true, activeShoot = true;
	private int pointNum = 0;
	private int bow;
	private Image[] bows;
	private Vector3? point = null;

	void Start ()
	{
		bows = new Image[bowCount];
		bow = bowCount;
		for (int i = 0; i < bowCount; i++) 
		{
			bows [i] = Instantiate (bowIndicatorPrefab, bowsIndicators.transform);
			bows [i].color = Color.white;
		}
		point = wayPoints [0].position;
	}

	void Update ()
	{
		if (point == null) {
			if (pointNum < wayPoints.Length)
				point = wayPoints [pointNum].position;
			else
				point = null;
		} else {
			transform.position += transform.forward * speed * Time.deltaTime;


			Transform parentOvr = ovrCamera.parent;
			ovrCamera.parent = null;

			Quaternion relativePos = Quaternion.LookRotation (point.Value - transform.position);
			Quaternion rotation = Quaternion.RotateTowards (transform.rotation, relativePos, angularVelocity * Time.deltaTime);
			transform.rotation = rotation;

			ovrCamera.parent = parentOvr;
			if (Vector3.Distance (transform.position, point.Value) < 1) {
				point = null;
				pointNum++;
			}
		}
		if (isReloaded) {
			if ((Input.GetKey (KeyCode.R) || OVRInput.Get (OVRInput.Button.DpadLeft)) && bow < bowCount) {
				isReloaded = false;
				StartCoroutine (ReloadBow ());
			} else if ((Input.GetMouseButtonDown (0) || OVRInput.Get (OVRInput.Button.DpadDown)) && activeShoot && bow > 0) {
				isReloaded = false;
				StartCoroutine (Reload ());
				GetComponent<AudioSource> ().Play ();
				bow--;
				bows [bow].color = Color.gray;
				for (int i = 0; i < shootStart.Length; i++)
					Instantiate (bullet.gameObject, shootStart [i].position, shootStart [i].rotation);
			}
		}
	}

	[ContextMenu ("Add all Player points")]
	public void AddAllPoints ()
	{
		List<GameObject> objs = new List<GameObject> ();
		objs.AddRange (GameObject.FindGameObjectsWithTag ("Player point"));
		wayPoints = new Transform[objs.Count];
		for (int i = 0; i < objs.Count; i++) {
			string name = objs [i].name;
			name = name.Substring (name.IndexOf ("(") + 1);
			name = name.Remove (name.Length - 1);
			wayPoints [int.Parse (name) - 1] = objs [i].transform;
		}
	}

	public void setShoot(bool isShooting){
		activeShoot = isShooting;
	}

	void OnDrawGizmos ()
	{
		if (wayPoints.Length < 1)
			return;

		Gizmos.color = Color.green;
		Vector3 lastPos = transform.position;
		for (int i = 0; i < wayPoints.Length; i++) {
			if (wayPoints [i].transform == null)
				continue;
			Gizmos.DrawLine (lastPos, wayPoints [i].transform.position);
			lastPos = wayPoints [i].transform.position;
		}
	}

	IEnumerator Reload ()
	{
		yield return new WaitForSeconds (reloadTime);
		isReloaded = true;
	}

	IEnumerator ReloadBow ()
	{
		Animator anim = bowsIndicators.GetComponent<Animator> ();
		if (anim != null)
		{
			anim.SetTrigger ("Reload");
		}
		yield return new WaitForSeconds (reloadBowTime);
		if (anim != null)
		{
			anim.SetTrigger ("ReloadComplete");
		}
		for (int i = 0; i < bowCount; i++) 
		{
			bows [i].color = Color.white;
		}
		bow = bowCount;
		isReloaded = true;
	}
}
