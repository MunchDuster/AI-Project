using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class NNTest : MonoBehaviour
{
	public static NNTest instance;

	[Header("References")]
	public Transform spawnPoint;
	public SavePanel savePanel;
	public GameObject walkerPrefab;

	[Space(10)]
	[Header("Debug curves")]
	public AnimationCurve bestCurve;
	public AnimationCurve similarityCurve;

	//NN layout
	public int[] innerLayers = new int[] { 2, 5, 1 };

	//Unity Events
	public UnityEvent<int> OnGenerationStart;
	public UnityEvent OnGenerationFinish;
	[Space(10)]
	public UnityEvent<int> OnBatchStart;
	public UnityEvent OnBatchFinish;
	[Space(10)]
	public UnityEvent<float> OnBatchProgress;
	public UnityEvent OnStarted;

	[Header("Settings")]
	[Space(10)]
	[Tooltip("Each generation runs in batches.")]
	public int batchSize = 300;
	[Tooltip("Total generation size = batchSize * batches")]
	public int batches = 5;
	[Tooltip("Time between each batch")]
	public float batchRunTime = 0.25f;
	[Tooltip("Number of best bots to be cloned from")]
	public int noOfBests;
	public bool autoStart = false;

	[Space(10)]
	[Tooltip("Probability from 0 to 1")]
	public float mutateWieghtsChance = 0.001f;
	public float mutateBiasesChance = 0.001f;

	//NN arrays
	private NeuralNetwork[] networks;
	private NeuralNetwork[] bests;

	//Walker robots
	private Walker[] walkers;

	private int generation = 0;
	private int batch = 0;

	private int[] layers;

	public NeuralNetwork GetBest() { return bests == null ? null : bests[0]; }
	public int GetGeneration() { return generation; }

	[HideInInspector] public bool walkersVisible = true;

	public void ToggleWalkersVisible()
	{
		walkersVisible = !walkersVisible;

		foreach (Walker walker in walkers)
		{
			foreach (Renderer renderer in walker.gameObject.GetComponentsInChildren<Renderer>())
			{
				renderer.enabled = walkersVisible;
			}
		}

		foreach (Walker walker in walkers)
		{
			foreach (Renderer renderer in walker.gameObject.GetComponentsInChildren<Renderer>())
			{
				renderer.enabled = true;
			}
		}
	}

	public void StartRunning()
	{
		OnStarted.Invoke();

		networks = new NeuralNetwork[batchSize * batches];
		bests = new NeuralNetwork[noOfBests];

		Walker.notLoseWalkers = new List<Walker>();
		Walker.walkers = new List<Walker>();

		Walker.onWalkerLose += CheckAnyLeft;

		StartCoroutine(Simulate());
	}

	public float? GetBatchBestScore()
	{
		return lastBatchBestScore;
	}
	float? lastBatchBestScore;

	void Start()
	{
		if (autoStart) StartRunning();
		instance = this;
	}

	private Coroutine batchGapCoroutine;
	private int stopped = 0;

	public void CheckAnyLeft()
	{
		if (Walker.notLoseWalkers.Count == 0 && stopped < 1000)
		{
			stopped++;
			//magic words for a magic man
			Debug.Log("Stopping!");
			StopCoroutine(batchGapCoroutine);
		}
	}

	private IEnumerator Simulate()
	{
		SpawnWalkers();
		InitNetworks();
		InitBests();

		yield return StartCoroutine(RunGeneration());

		while (true)
		{
			CreateNextGeneration();
			yield return StartCoroutine(RunGeneration());
		}
	}

	IEnumerator RunGeneration()
	{
		OnGenerationStart.Invoke(generation);
		for (batch = 0; batch < batches; batch++)
		{
			OnBatchStart.Invoke(batch);
			StartScoringBatch();
			yield return StartCoroutine(RunBatch());
			StopScoringBatch();
			RankBestOfBatch();
			OnBatchFinish.Invoke();
		}
		OnGenerationFinish.Invoke();
		generation++;
	}

	IEnumerator RunBatch()
	{
		float batchRunTimeSoFar = 0;
		while (batchRunTimeSoFar < batchRunTime)
		{
			batchRunTimeSoFar += Time.deltaTime;
			OnBatchProgress.Invoke(batchRunTimeSoFar);
			yield return null;
		}
	}

	private void SpawnWalkers()
	{
		walkers = new Walker[batchSize - 1];

		for (int i = 0; i < batchSize - 1; i++)
		{
			GameObject newWalkerGameObject = Instantiate(walkerPrefab, spawnPoint.position, spawnPoint.rotation);
			Walker newWalker = newWalkerGameObject.GetComponent<Walker>();
			walkers[i] = newWalker;
		}

		layers = new int[innerLayers.Length + 2];
		layers[0] = walkers[0].GetRequiredInputs();
		layers[layers.Length - 1] = walkers[0].GetRequiredOutputs();

		for (int i = 0; i < innerLayers.Length; i++)
		{
			layers[i + 1] = innerLayers[i];
		}


#if UNITY_EDITOR
		string str = "Layers: [";
		for (int i = 0; i < layers.Length; i++)
		{
			str += layers[i] + ",";
		}
		str += "]";
		Debug.Log(str);
#endif
	}

	private void InitNetworks()
	{
		for (int i = 0; i < networks.Length; i++)
		{
			networks[i] = new NeuralNetwork(layers);
		}
	}
	private void InitBests()
	{
		//Init the bests and batchBests so that there are no null values
		for (int b = 0; b < noOfBests; b++)
		{
			bests[b] = networks[b];
		}

		//If there is a loaded best, use it
		NeuralNetwork loadednetwork = savePanel == null ? null : savePanel.bestLoadedAndChosen;
		if (loadednetwork != null) bests[0] = loadednetwork;
	}

	[HideInInspector] public int[] mutations;
	private void CreateNextGeneration()
	{
		mutations = new int[networks.Length];
		for (int i = 0; i < networks.Length; i++)
		{
			//Choose any of best to be parent
			NeuralNetwork best = bests[Random.Range(0, noOfBests)];
			networks[i].fitness = 0;
			networks[i].CopySettings(best);
			mutations[i] = networks[i].MutateNet(mutateWieghtsChance, mutateBiasesChance);
		}
	}

	private void StartScoringBatch()
	{
		for (int batchIndex = 0; batchIndex < walkers.Length; batchIndex++)
		{
			walkers[batchIndex].network = networks[GetNetworksIndex(batchIndex)];
			walkers[batchIndex].StartScoring();
		}
	}
	private void StopScoringBatch()
	{
		for (int batchIndex = 0; batchIndex < walkers.Length; batchIndex++)
		{
			walkers[batchIndex].StopScoring();
			walkers[batchIndex].Reset();
		}
	}

	int GetNetworksIndex(int batchIndex)
	{
		return batchIndex + batch * batchSize;
	}

	//TODO: BETTER SORTING ALGORTHYM
	private void RankBestOfBatch()
	{
		lastBatchBestScore = float.MinValue;
		for (int batchIndex = 0; batchIndex < batchSize - 1; batchIndex++)
		{
			if (networks[GetNetworksIndex(batchIndex)].fitness > lastBatchBestScore) lastBatchBestScore = networks[GetNetworksIndex(batchIndex)].fitness;
		}

		for (int batchIndex = 0; batchIndex < noOfBests; batchIndex++)
		{
			bool replaced = false;
			for (int bestIndex = 0; bestIndex < noOfBests && !replaced; bestIndex++)
			{
				if (networks[GetNetworksIndex(batchIndex)].fitness > bests[bestIndex].fitness)
				{
					NeuralNetwork newNet = new NeuralNetwork(networks[GetNetworksIndex(batchIndex)], true);
					InsertValue<NeuralNetwork>(ref bests, newNet, bestIndex);
					replaced = true;
				}
			}
		}
	}

	private void InsertValue<T>(ref T[] arr, T val, int index)
	{
		for (int i = index + 1; i < arr.Length; i++)
		{
			arr[i] = arr[i - 1];
		}

		arr[index] = val;
	}
}