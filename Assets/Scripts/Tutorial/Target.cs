using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
	[Header("State Settings")]
	public int health = 30;
	public AudioSource deathAudio;
	public Image progressBar;

	private float multiplyProgress = 0;

	void Start ()
	{
		multiplyProgress = 1f / health;
	}

	public void SetDamage (int damage)
	{
		int cur = health;
		health -= damage;
		Debug.Log (multiplyProgress);
		progressBar.fillAmount = health * multiplyProgress;
		if (cur > 0 && health < 1) {
			progressBar.gameObject.SetActive (false);
			deathAudio.Play ();
		}
		//change skin or add particle
	}

}
