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
		Debug.Log (""+unitAlive);
		int hight = range.Length - 1;
		while (hight >= 0 && range [hight] > unitAlive) {
			hight--;
		}
		hight++;
		starScript.SetStar (hight);
		starScript.saveStar(hight);
		onComplete.Invoke ();
	}
}
