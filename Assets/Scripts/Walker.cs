using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
	public static List<Walker> walkers;
	public static List<Walker> notLoseWalkers;

	public delegate void OnWalkerLose();
	public static OnWalkerLose onWalkerLose;

	[HideInInspector] public bool lost = false;
	public NeuralNetwork network;
	public Transform body;

	public float maxVelocity = 100;

	public HingeJoint[] joints;

	[Space(10)]
	public float touchingGroundPunishment = 4;
	public float positionReward = 1;

	private bool scoring = true;
	private Rigidbody[] rigidbodies;
	private StartPoint[] startPoints;

	private struct StartPoint
	{
		public Transform transform;
		public Vector3 position;
		public Quaternion rotation;
		public Rigidbody rigidBody;

		public StartPoint(Transform transform, Rigidbody rigidBody)
		{
			this.transform = transform;
			this.position = transform.position;
			this.rotation = transform.rotation;
			this.rigidBody = rigidBody;
		}
	}

	public void Reset()
	{
		ResetTransforms();
		lost = false;
	}

	public void StartScoring()
	{
		scoring = true;
		notLoseWalkers.Add(this);
	}

	public void StopScoring()
	{
		scoring = false;
	}

	// Start is called before the first frame update
	private void Awake()
	{
		walkers.Add(this);
		if (!lost) notLoseWalkers.Remove(this);

		rigidbodies = gameObject.GetComponentsInChildren<Rigidbody>();
		RecordTransforms();
	}

	// FixedUpdate is called every physics update
	private void FixedUpdate()
	{
		if (scoring)
		{
			RunNetwork();
			Score();
		}
	}

	private void RecordTransforms()
	{
		List<StartPoint> startPointsList = new List<StartPoint>();
		foreach (Rigidbody child in transform.GetComponentsInChildren<Rigidbody>())
		{
			StartPoint startPoint = new StartPoint(child.transform, child);
			startPointsList.Add(startPoint);
		}
		startPoints = startPointsList.ToArray();
	}


	public int GetRequiredInputs()
	{
		int extraInputs = 3; //body rotation input
		return joints.Length + extraInputs;
	}
	public int GetRequiredOutputs()
	{
		return joints.Length;
	}


	float SimplifyRotation(float rotation)
	{
		return (rotation / 180) - 1f; //TUrns a 0 to 360 rotation into a 1 to -1 rotation
	}

	private void RunNetwork()
	{
		int inputsLength = network.layers[0];
		float[] inputs = new float[inputsLength];

		//Get inputs
		int i = 0;
		// for (int l = 0; l < limbs.Length; l++)
		// {
		// 	Rigidbody limb = limbs[l];

		// 	float relativeVelocity = -limb.angularVelocity.x;
		// 	inputs[i++] = relativeVelocity / 4f;
		// 	float angle = SimplifyRotation(limb.transform.localEulerAngles.x);
		// 	inputs[i++] = angle;
		// }

		//Head angle inputs
		float headXAngle = SimplifyRotation(body.eulerAngles.x);
		inputs[i++] = headXAngle;

		float headYAngle = SimplifyRotation(body.localEulerAngles.y);
		inputs[i++] = headYAngle;

		float headZAngle = SimplifyRotation(body.localEulerAngles.z);
		inputs[i++] = headZAngle;

		//Apply outputs
		for (int l = 0; l < joints.Length; l++)
		{
			HingeJoint joint = joints[l];
			inputs[i++] = joint.angle / 90f;
		}



		//Get outputs
		float[] outputs = network.Propagate(inputs);

		//Apply outputs
		for (int l = 0; l < joints.Length; l++)
		{
			HingeJoint joint = joints[l];
			float x = TranslateOutput(outputs[l]);
			JointMotor motor = joint.motor;
			motor.targetVelocity = x * maxVelocity;
			joint.motor = motor;
		}
	}

	private float TranslateOutput(float output)
	{
		return output;
	}

	float touchingScore = 0;

	private void Score()
	{
		float positionScore = body.position.z * positionReward;
		float totalScoreThisFrame = positionScore - touchingScore;

		network.fitness += totalScoreThisFrame * Time.fixedDeltaTime;
		touchingScore = 0;
	}

	public void OnTouch()
	{
		touchingScore = touchingGroundPunishment;
		lost = true;
		onWalkerLose();
	}

	private void ResetTransforms()
	{
		foreach (StartPoint startPoint in startPoints)
		{
			startPoint.rigidBody.velocity = Vector3.zero;
			startPoint.rigidBody.angularVelocity = Vector3.zero;
			startPoint.transform.position = startPoint.position;
			startPoint.transform.rotation = startPoint.rotation;
		}
	}
}
