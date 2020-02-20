using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
	Camera cam;
	public float movementSpeed = 200f;
	public float freeLookSensitivity = 3f;
	public float fastMovementSpeed = 1000f;
	public float zoomSensitivity = 100f;
	public float fastZoomSensitivity = 300f;
	private bool looking = false;
	public float maxY = 50f;
	
	void Start()
	{
		cam = GetComponent<Camera>();
		cam.transform.position = new Vector3(0, 720, -280);
		cam.transform.eulerAngles = new Vector3(55, -35, 0);
	}
	void Update()
    {
/*		maxY = cam.transform.localRotation.y;
		if(maxY > 50)
		{
			looking = false;
		}*/
		cam.farClipPlane = 7000;
		var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		var movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
		{
			transform.position = transform.position + (-transform.right * movementSpeed * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
		{
			transform.position = transform.position + (transform.right * movementSpeed * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
		{
			transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
		{
			transform.position = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.Q))
		{
			transform.position = transform.position + (transform.up * movementSpeed * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.E))
		{
			transform.position = transform.position + (-transform.up * movementSpeed * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
		{
			transform.position = transform.position + (Vector3.up * movementSpeed * Time.deltaTime);
		}

		if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
		{
			transform.position = transform.position + (-Vector3.up * movementSpeed * Time.deltaTime);
		}

		if (looking)
		{
			float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
			float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
			transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
		}

		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis != 0)
		{
			var zoomSensitivity = fastMode ? this.fastZoomSensitivity : this.zoomSensitivity;
			transform.position = transform.position + transform.forward * axis * zoomSensitivity;
		}

		if (Input.GetKeyDown(KeyCode.Mouse1))
		{
			StartLooking();
		}
		else if (Input.GetKeyUp(KeyCode.Mouse1))
		{
			StopLooking();
		}
	}

	void OnDisable()
	{
		StopLooking();
	}

	/// <summary>
	/// Enable free looking.
	/// </summary>
	public void StartLooking()
	{
		looking = true;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	/// <summary>
	/// Disable free looking.
	/// </summary>
	public void StopLooking()
	{
		looking = false;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}
}
