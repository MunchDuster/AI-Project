using UnityEngine;

[System.Serializable]
public class NeuralNetworkData
{
	//Meta
	public string name;
	public int generation;
	public int batchSize;
	public int batches;

	//NN data
	public float fitness;

	public int[] layers;
	public float[][] biases;
	public float[][][] weights;

	public NeuralNetworkData(NeuralNetwork network, string name, int generation, int batchSize, int batches)
	{
		this.fitness = network.fitness;
		this.name = name;
		this.generation = generation;
		this.batchSize = batchSize;
		this.batches = batches;

		//Copy layers
		layers = new int[network.layers.Length];
		System.Array.Copy(network.layers, layers, network.layers.Length);

		//Copy biases
		biases = new float[network.biases.Length][];
		for (int i = 0; i < network.neurons.Length; i++)
		{
			biases[i] = new float[network.biases[i].Length];
			System.Array.Copy(network.biases[i], biases[i], network.biases[i].Length);
		}

		//Copy weights
		weights = new float[network.weights.Length][][];
		for (int i = 0; i < network.weights.Length; i++)
		{
			weights[i] = new float[network.weights[i].Length][];
			for (int j = 0; j < network.weights[i].Length; j++)
			{
				weights[i][j] = new float[network.weights[i][j].Length];
				System.Array.Copy(network.weights[i][j], weights[i][j], network.weights[i][j].Length);
			}
		}
	}
}