using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Placeable : MonoBehaviour
{
	public static List<Placeable> all = new List<Placeable>();

    public Vector3 position { get {return transform.position; }}
	public Renderer renderer;
	public Material placingMaterial;

	protected Material oldMaterial;

	// Awake is called when the gameObject is activated
	private void Awake()
	{
		oldMaterial = renderer.material;
		renderer.material = placingMaterial;

		all.Add(this);

		renderer.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
	}

	void OnDestroy()
	{
		all.Remove(this);
	}

	public virtual void Setup()
	{
		renderer.material = oldMaterial;
		renderer.gameObject.layer = LayerMask.NameToLayer("Default");
	}
	public abstract void SetPosition(Vector3 position);
	public abstract void SetForward(Vector3 forward);

	public abstract void Connect(Placeable placeable);
	public virtual void EnablePhysics(bool enablePhysics)
	{
		foreach(Rigidbody body in GetComponentsInChildren<Rigidbody>())
		{
			body.isKinematic = !enablePhysics;
		}
	}
}
