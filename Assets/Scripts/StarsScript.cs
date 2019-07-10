using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarsScript : MonoBehaviour
{
	public int levelNum;
	public Sprite enableSprite, disableSprite;
	public Image[] stars;

	void Start ()
	{
		loadStar ();
	}

	public void SetStar (int num)
	{
		for (int i = 0; i < stars.Length; i++) {
			stars [i].sprite = i < num ? enableSprite : disableSprite;
		}
	}

	public void loadStar ()
	{
		if (PrefsManager.CompleteLevel >= levelNum)
			SetStar (PrefsManager.getLevelStar (levelNum));
		else
			SetStar (0);
	}

	public void saveStar (int num)
	{
		if(PlayerPrefs.GetInt (levelNum.ToString (), 0) < num)
		PlayerPrefs.SetInt (levelNum.ToString (), num+1);
	}
}
