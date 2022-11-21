using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildPanel : MonoBehaviour
{
	public GameObject jointPrefab;
	public GameObject limbPrefab;

	public delegate void OnEvent();
	public OnEvent OnUpdate;

	GameObject placingItemGameobject;
	Placeable placingItem;

    public void CreateJoint()
	{
		placingItemGameobject = Instantiate(jointPrefab);
		placingItem = placingItemGameobject.GetComponent<Placeable>();
		OnUpdate = UpdatePlacingItem;
	}
	public void CreateLimb()
	{
		placingItemGameobject = Instantiate(limbPrefab);
		placingItem = placingItemGameobject.GetComponent<Placeable>();
		OnUpdate = UpdatePlacingItem;
	}


	void UpdatePlacingItem()
	{
		bool positionGiven = false;
		Placeable placingOnPlaceable = null;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out RaycastHit hit, 100f))
		{
			positionGiven = true;
			placingItem.SetPosition(hit.point);
			placingItem.SetForward(hit.normal);

			placingOnPlaceable = hit.collider.gameObject.GetComponentInParent<Placeable>();
		}

		if(Input.GetMouseButtonDown(0))
		{
			if(positionGiven)
			{
				placingItem.Setup();

				if(placingOnPlaceable != null)
				{
					placingItem.Connect(placingOnPlaceable);
					placingOnPlaceable.Connect(placingItem);
				}
			}
			else
				Destroy(placingItemGameobject);
				

			OnUpdate = null;
		}
	}


	// Update is called every frame
	private void Update()
	{
		if(OnUpdate != null) OnUpdate();
	}

	public void TogglePhysics(bool enablePhysics)
	{
		foreach(Placeable placeable in Placeable.all)
		{
			placeable.EnablePhysics(enablePhysics);
		}
	}
}
