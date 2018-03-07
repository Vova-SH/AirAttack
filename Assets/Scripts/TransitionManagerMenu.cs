using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TransitionManagerMenu : MonoBehaviour {
	public MenuElementChanger lastPosition;

	public void changePosition(MenuElementChanger currentItem){
		if (lastPosition != null) {
			foreach (MenuElementChanger item in lastPosition.onActivateItems) {
				item.disable ();
			}
		}
		lastPosition = currentItem;
	}

	public void LoadScenes(int levelNum)
	{
		SceneManager.LoadSceneAsync ("Level"+levelNum);
	}
}
