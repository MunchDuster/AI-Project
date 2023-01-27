using UnityEngine;

public class NeuralNetwork
{
	private const float weightInitRange = 2f;
	private const float biasInitRange = 1f;

	public static int created;

	public int id;
	public float fitness;

	public int[] layers;
	public float[][] neurons;
	public float[][] biases;
	public float[][][] weights;

	private float Sigmoid(float num)
	{
		return (2 / (1 + (float)System.Math.Pow(2.71828f, -2 * num))) - 1;
	}

	private float MutateValue(float value)
	{
		float ran = Random.Range(0, 3);

		if (ran == 0)
		{
			value *= Random.Range(0.5f, 2f);
		}
		else if (ran == 1)
		{
			value += Random.Range(-1f, 1f);
		}
		else
		{
			value *= -1;
		}

		return value;
	}

	public NeuralNetwork(NeuralNetwork copyNetwork, bool copyFitness = false)
	{
		id = created;
		created++;

		//Copy data from copyNetwork
		CopySettings(copyNetwork, copyFitness);
	}

	public void CopySettings(NeuralNetwork copyNetwork, bool copyFitness = false)
	{
		//Optionally copy fitness
		fitness = (copyFitness) ? copyNetwork.fitness : 0;

		//Copy layers
		int layersLength = copyNetwork.layers.Length;
		layers = new int[layersLength];
		System.Array.Copy(copyNetwork.layers, layers, layersLength);

		//Copy biases
		CopyBiases(copyNetwork.biases);

		//Copy weights
		CopyWeights(copyNetwork.weights);

		//Init Neurons
		int neuronsLength = copyNetwork.neurons.Length;
		neurons = new float[neuronsLength][];
		for (int i = 0; i < neuronsLength; i++)
		{
			int neuronsRowLength = copyNetwork.neurons[i].Length;
			neurons[i] = new float[neuronsRowLength];
		}
	}

	public void CopyBiases(float[][] copyBiases)
	{
		int biasesLength = copyBiases.Length;
		biases = new float[biasesLength][];
		for (int i = 0; i < biasesLength; i++)
		{
			int biasesRowLength = copyBiases[i].Length;
			biases[i] = new float[biasesRowLength];
			System.Array.Copy(copyBiases[i], biases[i], biasesRowLength);
		}
	}
	public void CopyWeights(float[][][] copyWeights)
	{
		int weightsLength = copyWeights.Length;
		weights = new float[weightsLength][][];
		for (int i = 0; i < weightsLength; i++)
		{
			int weightsRowLength = copyWeights[i].Length;
			weights[i] = new float[weightsRowLength][];
			for (int j = 0; j < weightsRowLength; j++)
			{
				int weightsLastColLength = copyWeights[i][j].Length;
				weights[i][j] = new float[weightsLastColLength];
				System.Array.Copy(copyWeights[i][j], weights[i][j], weightsLastColLength);
			}
		}
	}

	public NeuralNetwork(int[] layers)
	{
		id = created;
		created++;

		//Init vars
		fitness = 0;
		this.layers = layers;

		//Init Neurons and biases
		int rows = layers.Length;

		neurons = new float[rows][];
		biases = new float[rows][];

		for (int row = 0; row < rows; row++)
		{
			int cols = layers[row];
			neurons[row] = new float[cols];
			biases[row] = new float[cols];

			for (int col = 0; col < cols; col++)
			{
				biases[row][col] = Random.Range(-biasInitRange, biasInitRange);

			}
		}

		//Init Weights
		weights = new float[neurons.Length - 1][][];

		for (int row = 1; row < rows; row++)
		{
			int cols = neurons[row].Length;
			weights[row - 1] = new float[cols][];

			for (int col = 0; col < cols; col++)
			{
				int lastCols = neurons[row - 1].Length;
				weights[row - 1][col] = new float[lastCols];

				for (int lastCol = 0; lastCol < lastCols; lastCol++)
				{
					weights[row - 1][col][lastCol] = Random.Range(-weightInitRange, weightInitRange);
				}
			}
		}
	}

	public float[] Propagate(float[] inputs)
	{
		//Set input neurons values
		for (int col = 0; col < neurons[0].Length; col++)
		{
			neurons[0][col] = Sigmoid(inputs[col]);
		}

		//Propagate
		for (int row = 1; row < neurons.Length; row++)
		{
			for (int col = 0; col < neurons[row].Length; col++)
			{
				//Add all inputs
				float sum = 0;
				for (int lastCol = 0; lastCol < neurons[row - 1].Length; lastCol++)
				{
					sum += weights[row - 1][col][lastCol] * neurons[row - 1][lastCol];
				}

				sum += biases[row][col];

				neurons[row][col] = Sigmoid(sum);
			}
		}

		//Collect outputs
		int lastRow = layers[layers.Length - 1];
		float[] outputs = new float[lastRow];
		for (int row = 0; row < outputs.Length; row++)
		{
			outputs[row] = neurons[neurons.Length - 1][row];
		}

		return outputs;
	}

	public int MutateNet(float chanceWeights, float chanceBiases)
	{
		int mutations = 0;

		for (int row = 1; row < neurons.Length; row++)
		{
			for (int col = 0; col < neurons[row].Length; col++)
			{
				for (int lastCol = 0; lastCol < neurons[row - 1].Length; lastCol++)
				{
					mutations++;

					//Mutate weights
					float rand = Random.Range(0f, 1f);

					if (rand < chanceWeights)
					{
						weights[row - 1][col][lastCol] = MutateValue(weights[row - 1][col][lastCol]);
						mutations++;
					}
				}

				//Mutate biases
				float ran = Random.value;
				if (ran < chanceBiases)
				{
					biases[row][col] = MutateValue(biases[row][col]);
					mutations++;
				}
			}
		}
		return mutations;
	}

	public float CompareSimilarity(NeuralNetwork other)
	{
		if (other == null) throw new System.ArgumentNullException("other");

		int identicalWeights = 0;
		int totalWeights = 0;


		for (int row = 1; row < neurons.Length; row++)
		{
			for (int col = 0; col < neurons[row].Length; col++)
			{
				for (int lastCol = 0; lastCol < neurons[row - 1].Length; lastCol++)
				{
					float thisNum = weights[row - 1][col][lastCol];
					float otherNum = other.weights[row - 1][col][lastCol];

					if (Mathf.Abs(thisNum - otherNum) < 0.00000001f)
					{
						identicalWeights++;
					}
					totalWeights++;
				}
			}
		}

		// int identicalBiases = 0;
		// int totalBiases = 0;

		// for (int row = 0; row < neurons.Length; row++)
		// {
		// 	for (int col = 0; col < neurons[row].Length; col++)
		// 	{
		// 		float thisNum = biases[row][col];
		// 		float otherNum = other.biases[row][col];

		// 		if (Mathf.Abs(thisNum - otherNum) < 0.00000001f)
		// 		{
		// 			identicalBiases++;
		// 		}

		// 		totalBiases++;
		// 	}
		// }

		Debug.Log("");

		Debug.Log("Similarity: " + identicalWeights + totalWeights);


		return (float)identicalWeights / (float)totalWeights;
	}
}