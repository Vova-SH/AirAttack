using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour {

  void Update() {
    if(OVRInput.Get(OVRInput.Button.None)) Debug.Log("OVRGET None");
    if(OVRInput.GetUp(OVRInput.Button.None)) Debug.Log("OVRGET Up None");
    if(OVRInput.GetDown(OVRInput.Button.None)) Debug.Log("OVRGET Down None");

    if(OVRInput.Get(OVRInput.Button.Any)) Debug.Log("OVRGET Any");
    if(OVRInput.GetUp(OVRInput.Button.Any)) Debug.Log("OVRGET Up Any");
    if(OVRInput.GetDown(OVRInput.Button.Any)) Debug.Log("OVRGET Down Any");

    if(OVRInput.Get(OVRInput.RawTouch.Any)) Debug.Log("OVRGET Any touch");
    if(OVRInput.GetUp(OVRInput.RawTouch.Any)) Debug.Log("OVRGET Up Any touch");
    if(OVRInput.GetDown(OVRInput.RawTouch.Any)) Debug.Log("OVRGET Down Any touch");

    if(OVRInput.GetUp(OVRInput.RawButton.Any)) Debug.Log("OVRGET Up Any raw");
    if(OVRInput.GetDown(OVRInput.RawButton.Any)) Debug.Log("OVRGET Down Any raw");
    if(OVRInput.Get(OVRInput.RawButton.Any)) Debug.Log("OVRGET Any touch raw");

    if(OVRInput.Get(OVRInput.Touch.One)) Debug.Log("OVRGET one");
    if(OVRInput.GetUp(OVRInput.Touch.One)) Debug.Log("OVRGET Up one");
    if(OVRInput.GetDown(OVRInput.Touch.One)) Debug.Log("OVRGET Down one");

  }

	void OnGUI () {
                float fps = 1.0f/Time.deltaTime;
                GUILayout.Label("FPS = " + fps);
				if(fps<50) Debug.LogError("FPS ="+fps);
				//else Debug.Log("FPS ="+fps);
        }
}
