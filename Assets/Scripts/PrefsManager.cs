using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PrefsManager {
	public static int CompleteLevel
	{
		get
		{
			return PlayerPrefs.GetInt ("CompleteLevel", 1);
		}
		set
		{
			PlayerPrefs.SetInt ("CompleteLevel", value);
		}
	}

	public static int getLevelStar(int levelNum)
	{
		return PlayerPrefs.GetInt (levelNum.ToString(), 0);
	}

	public static void setLevelStar(int levelNum, int value)
	{
		PlayerPrefs.SetInt (levelNum.ToString(), value);
	}
}
