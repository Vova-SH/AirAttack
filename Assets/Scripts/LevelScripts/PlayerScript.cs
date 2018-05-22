using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
	[Header("Panel Initialize")]
	public GameObject bulletsIndicators;
	public Image bulletIndicatorPrefab;
	public GameObject reloadIndicator;

	[Header("Move Settings")]
	public Spline spline;
	public float speed = 0.01f;

	[Header("Shoot Settings")]
	public GameObject bulletPrefab;
	public float reloadShootTime = 0.1f;
	public AudioSource shootSound;
	public ParticleSystem[] shootStart;

	[Header("Bullet Settings")]
	public int bulletCount = 10;
	public float reloadBulletTime = 2.0f;
	public AudioSource reloadSound;

	[Header("Other Initialize")]
	public Transform ovrCamera;

	private bool isReloaded = true, activeShoot = true;
	private int bulletCout;
	private Image[] bullets;
	private float progress = 0;

	void Start ()
	{
		bullets = new Image[bulletCount];
		bulletCout = bulletCount;
		for (int i = 0; i < bulletCount; i++) 
		{
			bullets [i] = Instantiate (bulletIndicatorPrefab, bulletsIndicators.transform);
			bullets [i].color = Color.white;
		}
	}

	void Update ()
	{
		progress += Time.deltaTime;
		transform.position = spline.GetPositionOnSpline( SplineMovable.WrapValue( progress * speed, 0f, 1f, WrapMode.Clamp ) );
		if (isReloaded) {
			if ((Input.GetKey (KeyCode.R) || OVRInput.Get (OVRInput.Button.DpadLeft)) && bulletCout < bulletCount) {
				isReloaded = false;
				reloadSound.Play ();
				StartCoroutine (ReloadBow ());
			} else if ((Input.GetMouseButtonDown (0) || OVRInput.Get (OVRInput.Touch.One)) && activeShoot && bulletCout > 0) {
				isReloaded = false;
				StartCoroutine (Reload ());
				shootSound.Play ();
				bulletCout--;
				bullets [bulletCout].color = Color.gray;
				for (int i = 0; i < shootStart.Length; i++)
					Instantiate (bulletPrefab.gameObject, shootStart [i].transform.position, shootStart [i].transform.rotation);
				if(bulletCout==0) reloadIndicator.SetActive(true);
			}
		}
	}

	public void setShoot(bool isShooting){
		activeShoot = isShooting;
	}

	IEnumerator Reload ()
	{
		for (int i = 0; i < shootStart.Length; i++)
		{
			shootStart[i].Play();
		}/*
		yield return new WaitForSeconds (0.03f);
		for (int i = 0; i < shootStart.Length; i++)
		{
			shootStart[i].Stop();
		}*/

		yield return new WaitForSeconds (reloadShootTime);
		isReloaded = true;
	}

	IEnumerator ReloadBow ()
	{
		reloadIndicator.SetActive(false);
		Animator anim = bulletsIndicators.GetComponent<Animator> ();
		if (anim != null)
		{
			anim.SetTrigger ("Reload");
		}
		yield return new WaitForSeconds (reloadBulletTime);
		if (anim != null)
		{
			anim.SetTrigger ("ReloadComplete");
		}
		for (int i = 0; i < bulletCount; i++) 
		{
			bullets [i].color = Color.white;
		}
		bulletCout = bulletCount;
		isReloaded = true;
	}
}
