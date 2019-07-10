using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class TransitionManagerMenu : MonoBehaviour {
	public MenuElementChanger lastPosition;

	public Animator hideEye;

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
		hideEye.Play("LoadLevel");
		SceneManager.LoadSceneAsync (levelNum);
	}
}
