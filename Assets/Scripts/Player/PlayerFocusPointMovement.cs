using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFocusPointMovement : MonoBehaviour
{
	public Transform focusPoint;

	public float maxDistance;
	public float minDistance;
	public float minAngle;
	public float maxAngle;

	[Space(10)]
	public float lookSensitivity = 1;
	public float zoomSensitivity = 1;

    private float xAngle = 10;
    private float yAngle;
    private float distance = 5;

    // Update is called once per frame
    void Update()
    {
		//Rotate
        if(Input.GetMouseButton(2))
		{
			float deltaX = lookSensitivity * Time.deltaTime * Input.GetAxis("Mouse X");
			float deltaY = lookSensitivity * Time.deltaTime * Input.GetAxis("Mouse Y");

			yAngle -= deltaX;
			xAngle = Mathf.Clamp(xAngle + deltaY, minAngle, maxAngle);

			transform.localPosition = new Vector3(Mathf.Cos(yAngle * Mathf.Deg2Rad), Mathf.Sin(xAngle * Mathf.Deg2Rad), Mathf.Sin(yAngle * Mathf.Deg2Rad)) * distance;
			transform.LookAt(focusPoint);
		}

		//Zoom
		float deltaD = -Input.mouseScrollDelta.y;
		if(deltaD != 0)
		{
			distance = Mathf.Clamp(distance + deltaD, minDistance, maxDistance);
			UpdatePosition();
		}
    }

	void UpdatePosition()
	{
		transform.localPosition = -transform.forward * distance;
	}
}
