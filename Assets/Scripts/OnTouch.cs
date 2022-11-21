using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTouch : MonoBehaviour
{
	public UnityEvent OnTouchEnter;
	public UnityEvent OnTouchStay;

	public new string tag;

	// OnCollisionEnter is called when a collider on this gameobject collides with another
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.tag == tag)
		{
			OnTouchEnter.Invoke();
		}
	}

	// OnCollisionEnter is called when a collider on this gameobject collides with another
	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.tag == tag)
		{
			OnTouchStay.Invoke();
		}
	}
}
