using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : Placeable
{
	public Transform cylinder;
    public HingeJoint hinge;

	AIJoint joint0;
	AIJoint joint1;
	float length = 1;

	public void SetJoint0(AIJoint joint)
	{
		joint0 = joint;
		UpdateLimb();
	}

	public void SetJoint1(AIJoint joint)
	{
		joint1 = joint;
		UpdateLength();
		UpdateLimb();
	}

	void UpdateLength()
	{
		length = Vector3.Distance(joint0.position, joint1.position);
		cylinder.localScale = new Vector3(0.2f, length, 0.2f);
	}

    void UpdateLimb()
	{
		cylinder.position = (joint0.position + joint1.position) / 2f;
		cylinder.rotation = Quaternion.LookRotation((joint1.position - joint0.position).normalized);
	}

	public override void Connect(Placeable placeable)
	{
		
	}

	public override void SetPosition(Vector3 position)
	{
		transform.position = position;
	}
	public override void SetForward(Vector3 forward)
	{
		transform.rotation = Quaternion.LookRotation(forward);
	}

	public override void EnablePhysics(bool enablePhysics)
	{
		if(hinge.connectedBody == null) Destroy(hinge);
		base.EnablePhysics(enablePhysics);
	}
}
