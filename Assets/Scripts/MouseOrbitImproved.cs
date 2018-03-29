using System.Collections;
using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour {

	#if UNITY_EDITOR
	public Transform target;
	public float distance = 5.0f;
	public float xSpeed = 120.0f;
	public float ySpeed = 120.0f;

	public float yMinLimit = -20f;
	public float yMaxLimit = 80f;

	float x = 0.0f;
	float y = 0.0f;

	// Use this for initialization
	void Start () 
	{
		Vector3 angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;

		Rigidbody rigidbody = GetComponent<Rigidbody>();

		// Make the rigid body not change rotation
		if (rigidbody != null)
		{
			rigidbody.freezeRotation = true;
		}
	}

	void LateUpdate () 
	{
		if (target && Input.GetKey(KeyCode.LeftAlt)) 
		{
			Cursor.lockState = CursorLockMode.Locked;
			x += Input.GetAxis ("Mouse X") * xSpeed * distance * 0.02f;
			y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

			y = ClampAngle(y, yMinLimit, yMaxLimit);

			transform.rotation = Quaternion.Euler(y, x, 0);
		} else Cursor.lockState = CursorLockMode.None;
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		angle %= 360F;
		return Mathf.Clamp(angle, min, max);
	}
	#endif
}