using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIJoint : Placeable
{
	public Limb part0;
	public Limb part1;
	
	public override void SetPosition(Vector3 position)
	{
		transform.position = position;
	}
	public override void SetForward(Vector3 forward)
	{
		transform.position += forward * 0.1f;
	}
	public override void Connect(Placeable placeable)
	{
		if(placeable.GetType() == typeof(Limb)) Debug.Log("Limb!");

		Limb limb = placeable as Limb;

		if(part0 == null)
		{
			part0 = limb;
			transform.SetParent(part0.transform);
		}
		else 
		{
			part1 = limb;
			part0.hinge.connectedBody = part1.GetComponent<Rigidbody>();
		}
	}
}
