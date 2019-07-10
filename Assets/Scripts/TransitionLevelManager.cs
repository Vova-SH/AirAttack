using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class TransitionLevelManager : MonoBehaviour {
	public StarsScript starScript;
	public int[] range;
	public UnityEvent onComplete;

	public void restartLevel(){
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}

	public void loadLevel(int num){
		SceneManager.LoadSceneAsync (num);
	}

	public void showMenu(int unitAlive) {
		int hight = range.Length - 1;
		while (hight >= 0 && range [hight] > unitAlive) {
			hight--;
		}
		hight++;
		starScript.SetStar (hight+1);
		starScript.saveStar(hight+1);
		if(PrefsManager.CompleteLevel < starScript.levelNum+1) PrefsManager.CompleteLevel=starScript.levelNum+1;
		onComplete.Invoke ();
	}
}
