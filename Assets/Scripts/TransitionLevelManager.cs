using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class TransitionLevelManager : MonoBehaviour {

	public UnityEvent onComplete;

	public void restartLevel(){
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}

	public void loadLevel(int num){
		SceneManager.LoadSceneAsync (num);
	}

	public void showMenu() {
		onComplete.Invoke ();
	}
}
