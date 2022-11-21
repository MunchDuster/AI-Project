using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class SaveFileUI : MonoBehaviour
{
	public SavePanel savePanel;
	public TextMeshProUGUI nameText;
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI generationText;

	public NeuralNetworkData data;

	public void Setup(NeuralNetworkData data, SavePanel savePanel)
	{
		this.data = data;
		this.savePanel = savePanel;

		nameText.text = data.name;
		scoreText.text = data.fitness.ToString();
		generationText.text = data.generation.ToString() + "/" + (data.batchSize * data.batches).ToString();
	}

	public void ButtonClicked()
	{
		savePanel.SetChosen(this);
	}
}