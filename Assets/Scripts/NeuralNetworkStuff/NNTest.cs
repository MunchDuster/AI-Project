using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NNTest : MonoBehaviour
{
	public Transform spawnPoint;
	public SavePanel savePanel;

	[Space(10)]
	public AnimationCurve bestCurve;
	public AnimationCurve similarityCurve;

	public int[] innerLayers = new int[] { 2, 5, 1 };

	public delegate void OnIntEvent(int num);
	public event OnIntEvent OnGenerationCompleted;
	public event OnIntEvent OnBatchCompleted;
	public delegate void OnEvent();
	public event OnEvent OnGenerationStarted;
	public event OnEvent OnStartedRunning;

	[Space(10)]
	[Tooltip("Each generation runs in batches.")]
	public int batchSize = 300;
	[Tooltip("Total generation size = batchSize * batches")]
	public int batches = 5;
	[Tooltip("Time between each batch")]
	public float batchGap = 0.25f;
	[Tooltip("Number of best bots to be cloned from")]
	public int noOfBests;

	[Space(10)]
	[Tooltip("Probability from 0 to 1")]
	public float mutateWieghtsChance = 0.001f;
	public float mutateBiasesChance = 0.001f;

	[Space(10)]
	public GameObject walkerPrefab;

	private NeuralNetwork[] networks;
	private NeuralNetwork[] batchBests;
	private NeuralNetwork[] bests;

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
		OnStartedRunning();

		networks = new NeuralNetwork[batchSize];
		bests = new NeuralNetwork[noOfBests];
		batchBests = new NeuralNetwork[noOfBests];

		Walker.notLoseWalkers = new List<Walker>();
		Walker.walkers = new List<Walker>();

		Walker.onWalkerLose += CheckAnyLeft;

		StartCoroutine(Simulate());
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
		InitBests();
		OnGenerationStarted();
		//Run first generation outside of loop (no best to use yet)
		for (batch = 0; batch < batches; batch++)
		{
			InitBatch();
			StartScoringBatch();
			yield return batchGapCoroutine = StartCoroutine(batchGapWait());
			StopScoringBatch();
			RankBestOfBatch();
			OnBatchCompleted(batch);
		}
		RankBestOfGeneraton();
		OnGenerationCompleted(generation);

		while (true)
		{
			generation++;

			for (batch = 0; batch < batches; batch++)
			{
				PrepareBatch();
				StartScoringBatch();
				yield return batchGapCoroutine = StartCoroutine(batchGapWait());
				StopScoringBatch();
				RankBestOfBatch();
				OnBatchCompleted(batch);

			}
			RankBestOfGeneraton();
			OnGenerationCompleted(generation);

		}
	}
	private IEnumerator batchGapWait()
	{
		yield return new WaitForSeconds(batchGap);
	}

	private void SpawnWalkers()
	{
		walkers = new Walker[networks.Length];

		for (int i = 0; i < networks.Length; i++)
		{
			GameObject newWalkerGameObject = Instantiate(walkerPrefab, spawnPoint.position, spawnPoint.rotation);
			Walker newWalker = newWalkerGameObject.GetComponent<Walker>();
			walkers[i] = newWalker;
		}

		layers = new int[innerLayers.Length + 2];
		layers[0] = walkers[0].GetRequiredInputs();
		layers[layers.Length - 1] = walkers[0].GetRequiredOutputs();
	}

	private void InitBatch()
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
			bests[b] = new NeuralNetwork(layers);
			bests[b].fitness = float.MinValue;

			batchBests[b] = new NeuralNetwork(layers);
			batchBests[b].fitness = float.MinValue;
		}

		//If there is a loaded best, use it
		NeuralNetwork loadednetwork = savePanel.bestLoadedAndChosen;
		if (loadednetwork != null) bests[0] = loadednetwork;
	}
	private void PrepareBatch()
	{
		for (int i = 0; i < networks.Length; i++)
		{
			//Choose any of best to be parent
			NeuralNetwork best = bests[Random.Range(0, noOfBests)];

			networks[i].fitness = 0;
			networks[i].CopySettings(best);
			networks[i].MutateNet(mutateWieghtsChance, mutateBiasesChance);
		}
	}

	private void StartScoringBatch()
	{
		for (int i = 0; i < networks.Length; i++)
		{
			walkers[i].network = networks[i];
			walkers[i].StartScoring();
		}
	}
	private void StopScoringBatch()
	{
		for (int i = 0; i < networks.Length; i++)
		{
			walkers[i].StopScoring();
			walkers[i].Reset();
		}
	}

	private void RankBestOfBatch()
	{
		for (int i = 0; i < networks.Length; i++)
		{
			for (int b = 0; b < noOfBests; b++)
			{
				if (networks[i].fitness > batchBests[b].fitness)
				{
					NeuralNetwork newNet = new NeuralNetwork(networks[i], true);
					InsertValue<NeuralNetwork>(ref batchBests, newNet, b);
				}
			}
		}

		Debug.Log("Batch bests:");
		for (int g = 0; g < noOfBests; g++)
		{
			Debug.Log("Batch bests[" + g + "]: " + batchBests[g].fitness);
		}
	}
	private void RankBestOfGeneraton()
	{
		for (int b = 0; b < noOfBests; b++)
		{
			for (int g = 0; g < noOfBests; g++)
			{
				if (batchBests[b].fitness > bests[g].fitness)
				{
					NeuralNetwork newNet = new NeuralNetwork(batchBests[b], true);
					InsertValue<NeuralNetwork>(ref bests, newNet, g);
				}
			}
		}

		Debug.Log("Generations bests:");
		for (int g = 0; g < noOfBests; g++)
		{
			Debug.Log("Bests[" + g + "]: " + bests[g].fitness);
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