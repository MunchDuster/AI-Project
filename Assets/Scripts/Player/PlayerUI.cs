using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
	public static PlayerUI ui;

	public NNTest tester;

	public UnityEvent OnPause;
	public UnityEvent OnUnpause;

	public Slider generationCompletenessSlider;

	public TextMeshProUGUI errorText;
	public TextMeshProUGUI messageText;
	public TextMeshProUGUI fpsText;

	public TextMeshProUGUI generationText;
	public TextMeshProUGUI scoreText;

	private float generationTime;
	private float timeSinceLastGeneration = 0;

	private Coroutine errorCoroutine;
	private Coroutine messageCoroutine;


	// Start is called before the first frame update
	void Start()
	{
		ui = this;

		tester.OnGenerationStarted += OnGenerationCompleted;
		tester.OnGenerationCompleted += OnGenerationCompleted;
		tester.OnBatchCompleted += OnBatchCompleted;
		tester.OnStartedRunning += OnStartedRunning;
	}

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

	private IEnumerator HideText(TextMeshProUGUI text)
	{
		yield return new WaitForSeconds(3);
		text.text = "";
	}

	private void OnStartedRunning()
	{
		generationTime = tester.batches * tester.batchGap;
	}

	private void OnGenerationCompleted()
	{
		generationText.text = "Generation 1";
		generationCompletenessSlider.value = 0;

		NeuralNetwork best = tester.GetBest();
		scoreText.text = (best == null) ? "[no best yet]" : "Best " + best.fitness.ToString("0.0");
	}
	private void OnGenerationCompleted(int generation)
	{
		generationText.text = "Generation " + (generation + 1);
		generationCompletenessSlider.value = 0;

		NeuralNetwork best = tester.GetBest();
		scoreText.text = (best == null) ? "[no best yet]" : "Best " + best.fitness.ToString("0.0");
	}

	private float timeSinceUpdated = 0;
	private bool isPaused = false;

	private void OnBatchCompleted(int batch)
	{
		//Generation slider
		generationCompletenessSlider.value = batch / tester.batches;
	}
	private void Update()
	{

		//Pausing
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			isPaused = !isPaused;

			if (isPaused) OnPause.Invoke();
			else OnUnpause.Invoke();
		}

		//FPS
		timeSinceUpdated += Time.deltaTime;
		if (timeSinceUpdated >= 0.5f)
		{
			float fps = 1f / Time.deltaTime;
			fpsText.text = "FPS " + fps;
			timeSinceUpdated = 0;
		}
	}
}
