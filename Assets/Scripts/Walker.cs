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

	public Rigidbody head;
	public Rigidbody[] limbs;
	public HingeJoint[] joints;

	public bool debug;

	[Space(10)]
	public float touchingGroundPunishment = 4;
	public float movingForwardReward = 2;
	public float positionReward = 1;

	private bool scoring = true;
	private Rigidbody[] rigidbodies;
	private StartPoint[] startPoints;

	private float totalVelocityScore = 0;
	private float totalTouchingScore = 0;

	private struct StartPoint
	{
		public Transform transform;
		public Vector3 position;
		public Quaternion rotation;

		public StartPoint(Transform transform)
		{
			this.transform = transform;
			this.position = transform.position;
			this.rotation = transform.rotation;
		}
	}

	public void Reset()
	{
		ResetTransforms();
		lost = false;
		totalVelocityScore = 0;
		totalTouchingScore = 0;
	}

	public void StartScoring()
	{
		scoring = true;
		notLoseWalkers.Add(this);
		SetRigidbodiesKinematic(false);
	}

	public void StopScoring()
	{
		scoring = false;
		SetRigidbodiesKinematic(true);
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
		foreach (Transform child in transform.GetComponentsInChildren<Transform>())
		{
			StartPoint startPoint = new StartPoint(child);
			startPointsList.Add(startPoint);
		}
		startPoints = startPointsList.ToArray();
	}


	public int GetRequiredInputs()
	{
		int extraInputs = 2 /*head Y & Z angles*/;
		return limbs.Length * 2 + extraInputs;
	}
	public int GetRequiredOutputs()
	{
		return joints.Length * 1;
	}

	private void RunNetwork()
	{
		int inputsLength = network.layers[0];
		float[] inputs = new float[inputsLength];

		//Get inputs
		int i = 0;
		for (int l = 0; l < limbs.Length; l++)
		{
			Rigidbody limb = limbs[l];


			float relativeVelocity = -limb.angularVelocity.x;
			inputs[i++] = relativeVelocity;
			float angle = limb.transform.localEulerAngles.x / 180f - 1f;
			inputs[i++] = angle / 180f;
		}

		float headYAngle = head.transform.localEulerAngles.y / 180f - 1f;
		inputs[i++] = headYAngle;

		float headZAngle = head.transform.localEulerAngles.z / 180f - 1f;
		inputs[i++] = headZAngle;

		//Get outputs
		float[] outputs = network.Propagate(inputs);

		//Apply outputs
		i = 0;
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

	private void Score()
	{
		float positionScore = body.position.z;
		totalVelocityScore += head.velocity.z * Time.fixedDeltaTime;

		network.fitness = positionScore * positionReward + totalVelocityScore * movingForwardReward - totalTouchingScore * touchingGroundPunishment;
	}

	public void OnTouch()
	{
		totalTouchingScore += touchingGroundPunishment;

		//lost = true;
		//onWalkerLose();
	}

	private void ResetTransforms()
	{
		foreach (StartPoint startPoint in startPoints)
		{
			startPoint.transform.position = startPoint.position;
			startPoint.transform.rotation = startPoint.rotation;
		}
	}

	private void SetRigidbodiesKinematic(bool kinematic)
	{
		foreach (Rigidbody rb in rigidbodies)
		{
			rb.isKinematic = kinematic;
		}
	}
}
