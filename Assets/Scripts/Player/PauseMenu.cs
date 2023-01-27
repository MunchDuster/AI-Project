using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
	public UnityEvent OnPause;
	public UnityEvent OnUnpause;

	private bool isPaused = false;


	private void Update()
	{

		//Pausing
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			isPaused = !isPaused;

			if (isPaused) OnPause.Invoke();
			else OnUnpause.Invoke();
		}

	}
}