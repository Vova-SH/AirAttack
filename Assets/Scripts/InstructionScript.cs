using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InstructionScript : MonoBehaviour {
	
	public InstructionData[] data;
	public Image imageView;
	public Text textView;

	private int current = 0;

	void Start()
	{
		if (data.Length > 0) showData ();
	}

	public void setNextInstrucion()
	{
		current++;
		if (current >= data.Length)
			current = 0;
		showData ();
	}

	public void setPrevInstrucion()
	{
		current--;
		if (current < 0)
			current = data.Length - 1;
		showData ();
	}

	private void showData()
	{
		imageView.sprite = data [current].sprite;
		textView.text = data [current].text;		
	}
}
