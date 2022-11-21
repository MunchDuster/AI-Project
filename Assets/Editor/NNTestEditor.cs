using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(NNTest))]
public class NNTestEditor : Editor
{
	public override void OnInspectorGUI()
	{
		NNTest tester = (NNTest)target;

		string buttonText = (tester.walkersVisible) ? "Hide walkers" : "Show walkers";

		if (GUILayout.Button(buttonText))
		{
			tester.ToggleWalkersVisible();
		}

		DrawDefaultInspector();
	}
}
