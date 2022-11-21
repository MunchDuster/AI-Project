using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float moveSpeed = 50;
	public float slowMoveSpeed = 10;
	public float rotateSpeed = 100;

	// Update is called once per frame
	void Update()
	{
		Vector3 moveAmount = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
		float speed = (Input.GetKey(KeyCode.LeftShift)) ? slowMoveSpeed : moveSpeed;
		transform.position += moveAmount * Time.deltaTime * speed;

		Vector3 rotateAmount = Vector3.up * Input.GetAxis("Mouse X");
		transform.rotation *= Quaternion.Euler(rotateAmount * Time.deltaTime * rotateSpeed);
	}
}
