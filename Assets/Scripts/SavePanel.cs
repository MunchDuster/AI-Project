using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SavePanel : MonoBehaviour
{
	public TMP_InputField nameInput;

	public TextMeshProUGUI saveLocationText;

	public Transform saveFileUIParent;
	public GameObject saveFileUIPrefab;

	public NNTest tester;

	public NeuralNetwork bestLoadedAndChosen;

	public List<NeuralNetworkData> loadedData = new List<NeuralNetworkData>();
	public List<NeuralNetworkData> shownData = new List<NeuralNetworkData>();

	// Start is called before the first frame update
	private void Start()
	{
		saveLocationText.text = "Save location: " + SaveSystem.GetPath();
	}

	public void SetChosen(SaveFileUI saveFile)
	{
		ErrorUI.ui.Message(saveFile.data.name + " chosen.");

		NeuralNetwork network = new NeuralNetwork(saveFile.data.layers);

		network.CopyBiases(saveFile.data.biases);
		network.CopyWeights(saveFile.data.weights);

		bestLoadedAndChosen = network;
	}

	public void Save()
	{
		NeuralNetwork network = tester.GetBest();

		if (network == null)
		{
			ErrorUI.ui.Error("No network to save!");
			return;
		}

		string name = nameInput.text;
		int generation = tester.GetGeneration();
		int batchSize = tester.batchSize;
		int batches = tester.batches;

		NeuralNetworkData data = new NeuralNetworkData(network, name, generation, batchSize, batches);

		bool success = SaveSystem.SaveFile(data);

		if (success)
		{
			Debug.Log("Save successful");
			ErrorUI.ui.Message("Save successful.");

			loadedData.Add(data);
		}
		else
		{
			ErrorUI.ui.Error("Save failed.");
			Debug.Log("Save failed");
		}

		Refresh();
	}

	public void Load()
	{
		loadedData.Clear();

		string[] fileNames = SaveSystem.GetFileNamesInDirectory();

		foreach (string fileName in fileNames)
		{
			Debug.Log("File name: " + fileName);
			NeuralNetworkData data = SaveSystem.LoadFile(fileName);

			if (data != null)
			{
				ErrorUI.ui.Message("Load Successful.");
				Debug.Log("Load successful");
				loadedData.Add(data);
			}
			else
			{
				ErrorUI.ui.Error("Load failed.");
				Debug.Log("Load failed");
			}
		}

		Refresh();
	}

	public void Refresh()
	{
		ClearSavePanel();
		foreach (NeuralNetworkData data in loadedData)
		{
			GameObject saveFileUIGameObject = Instantiate(saveFileUIPrefab, saveFileUIParent);

			SaveFileUI saveFileUI = saveFileUIGameObject.GetComponent<SaveFileUI>();

			saveFileUI.Setup(data, this);
		}
	}

	private void ClearSavePanel()
	{
		foreach (Transform child in saveFileUIParent)
		{
			Destroy(child.gameObject);
		}
	}
}
