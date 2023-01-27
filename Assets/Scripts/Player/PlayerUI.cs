using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ErrorUI : MonoBehaviour
{
	public static ErrorUI ui;

	public TextMeshProUGUI errorText;
	public TextMeshProUGUI messageText;

	Coroutine errorCoroutine;
	Coroutine messageCoroutine;

	public void Error(string message)
	{
		if (errorCoroutine != null) StopCoroutine(errorCoroutine);

		errorText.text = message;
		errorCoroutine = StartCoroutine(HideText(errorText));
	}

	public void Message(string message)
	{
		if (messageCoroutine != null) StopCoroutine(messageCoroutine);

		messageText.text = message;
		messageCoroutine = StartCoroutine(HideText(messageText));
	}

	IEnumerator HideText(TextMeshProUGUI text)
	{
		yield return new WaitForSeconds(3);
		text.text = "";
	}
}
