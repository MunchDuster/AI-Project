using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScaleUI : MonoBehaviour
{
	public Slider slider;
	public TextMeshProUGUI text;

	// Update is called once per frame
	void Update()
	{
		float scale = Time.deltaTime / Time.unscaledDeltaTime;
		slider.value = scale;
		text.text = "Time scale: " + scale.ToString("F2");
	}
}
