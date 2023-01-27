using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Graph : MonoBehaviour
{
	[Header("References")]
	public RawImage rawImage;
	public TextMeshProUGUI maxText;
	public TextMeshProUGUI minText;
	public TextMeshProUGUI peakGenText;

	[Header("Image settings")]
	public int imageX;
	public int imageY;

	Texture2D texture;

	public struct Bar
	{
		public float value;
		public Color32 color;
		public int generation;

		public Bar(float value, Color32 color, int generation)
		{
			this.value = value;
			this.color = color;
			this.generation = generation;
		}
	}

	public List<Bar> values = new List<Bar>();

	public Color32 backGroundColor = new Color32(5, 5, 5, 255);
	public Color32 backGroundLineColor = new Color32(90, 90, 90, 255);
	public Color32 barMainColor = new Color32(50, 50, 200, 255);
	public Color32 barSecColor = new Color32(100, 100, 200, 255);

	Color32 barColor;
	bool isMainColor = true;

	void Start()
	{
		texture = new Texture2D(imageX, imageY);
		rawImage.texture = texture;
		barColor = barMainColor;
		DrawImage();
	}

	public void SwitchColor()
	{
		isMainColor = !isMainColor;
		if (isMainColor) barColor = barMainColor;
		else barColor = barSecColor;
	}

	public void AddValue(float value)
	{
		values.Add(new Bar(value, barColor, NNTest.instance.GetGeneration()));
		DrawImage();
	}

	void DrawImage()
	{
		//Bounds for details in image
		int maxY = imageY - 5;
		int minY = 5;

		//Setup image
		Color32[] pixels = new Color32[imageX * imageY];

		//Clear background
		for (int x = 0; x < imageX; x++)
		{
			for (int y = 0; y < imageY; y++)
			{
				pixels[x + y * imageX] = backGroundColor;
			}
		}

		//Draw background lines
		int lines = 5;
		for (int i = 0; i < lines; i++)
		{
			int y = Mathf.RoundToInt(Mathf.Lerp(minY, maxY, (float)i / (lines - 1)));
			for (int x = 0; x < imageX; x++)
			{
				pixels[x + Mathf.FloorToInt(y) * imageX] = backGroundLineColor;
			}
		}

		//Stop if no values to draw
		if (values == null || values.Count == 0)
		{
			texture.SetPixels32(pixels);
			texture.Apply();
			maxText.text = "Max: (none)";
			minText.text = "Min: (none)";
			return;
		}

		//Get min and max values
		Bar maxValue = new Bar(float.MinValue, barColor, -1);
		Bar minValue = new Bar(float.MaxValue, barColor, -1);

		for (int i = 0; i < values.Count; i++)
		{
			if (values[i].value > maxValue.value) maxValue = values[i];
			if (values[i].value < minValue.value) minValue = values[i];
		}

		//Update text
		maxText.text = "Max: " + maxValue.value.ToString("F2");
		minText.text = "Min: " + minValue.value.ToString("F2");
		peakGenText.text = "Peak Gen: " + maxValue.generation.ToString();

		//Draw rectangles
		int minBarHeight = 3;
		int barWidth = 7;
		int barGap = 2;
		int curX = 6;

		for (int i = values.Count - 1; i >= 0; i--)
		{
			float heightLerp = Mathf.InverseLerp(minValue.value, maxValue.value, values[i].value);
			int height = Mathf.RoundToInt(Mathf.Lerp(minY + minBarHeight, maxY, heightLerp));
			for (int x = curX; x < curX + barWidth; x++)
			{
				for (int y = minY; y < height; y++)
				{
					pixels[x + y * imageX] = values[i].color;
				}
			}
			curX += barWidth + barGap;

			if (curX >= imageX) break;
		}

		//Apply to texture
		texture.SetPixels32(pixels);
		texture.Apply();
	}
}