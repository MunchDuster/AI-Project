using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;

public class NNTesterUI : MonoBehaviour
{
	public TextMeshProUGUI generationNumberText;
	public TextMeshProUGUI batchNumberText;
	public TextMeshProUGUI fpsText;
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI generationProgressBarPercentText;
	public Slider batchProgressBar;
	public Graph scoreGraph;
	public Graph mutationsGraph;
	public NNTest nnTest;

	public void OnGenerationStart(int generation)
	{
		generationNumberText.text = "Gen: " + (generation + 1);
	}

	public void OnGenerationFinish()
	{
		if (nnTest.mutations == null || nnTest.mutations.Length == 0) return;
		float avg = (float)Queryable.Average(nnTest.mutations.AsQueryable());
		mutationsGraph.AddValue(avg);
	}
	public void OnBatchStart(int batch)
	{
		batchNumberText.text = "Batch: " + (batch + 1).ToString() + "/" + nnTest.batches;
	}
	public void OnBatchFinish()
	{
		float? score = nnTest.GetBatchBestScore();
		if (score == null) return;
		scoreGraph.AddValue((float)score);
	}

	public void UpdateBatchProgress(float progress)
	{
		batchProgressBar.value = progress / nnTest.batchRunTime;
		generationProgressBarPercentText.text = Mathf.CeilToInt(nnTest.batchRunTime - progress).ToString() + "s";
	}

	void Update()
	{
		fpsText.text = "FPS: " + Mathf.RoundToInt(1f / Time.unscaledDeltaTime).ToString();
	}
}