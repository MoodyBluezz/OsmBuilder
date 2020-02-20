using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class InputScript : MonoBehaviour
{
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HitScan();
		}
	}

	private void HitScan()
	{
		RaycastHit hit;
		Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit))
		{
			Transform objectHit = hit.transform;
			objectHit.gameObject.GetComponent<Renderer>().material = Resources.Load("Red", typeof(Material)) as Material;
			objectHit.gameObject.transform.localScale = new Vector3(transform.localScale.x, 2, transform.localScale.z);
			//objectHit.gameObject.transform.position = new Vector3(transform.position.x, 10, transform.position.z);
		}
	}
}
