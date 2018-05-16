using UnityEngine;
using UnityEngine.UI;

public class ChangeImageColor : MonoBehaviour {
	public Image img;
	public Color[] color;
	public void changeColor(int colorNum)
	{
		img.color = color[colorNum];
	}
}
