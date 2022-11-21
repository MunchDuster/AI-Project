using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
	public NNTest tester;
	public PlayerUI ui;
	public TMP_InputField batchSizeInput;
	public TMP_InputField batchesInput;
	public TMP_InputField batchTimeInput;
	public TMP_InputField mutationChanceInput;

	public void UpdateBatchSize()
	{
		SetPositiveInteger("batch size", batchSizeInput.text, ref tester.batchSize);
	}
	public void UpdateBatches()
	{
		SetPositiveInteger("batches", batchesInput.text, ref tester.batches);
	}
	public void UpdateBatchTime()
	{
		SetPositiveFloat("batch time", batchTimeInput.text, ref tester.batchGap);
	}
	public void UpdateMutationChance()
	{
		SetPositiveFloat("mutation chance", mutationChanceInput.text, ref tester.mutateWieghtsChance);
		SetPositiveFloat("mutation chance", mutationChanceInput.text, ref tester.mutateBiasesChance);
	}


	private void SetPositiveInteger(string name, string input, ref int value)
	{
		try
		{
			int intInput = int.Parse(input);

			if (intInput < 0)
			{
				ui.Error("Invalid " + name);
			}
			else
			{
				value = intInput;
			}
		}
		catch
		{
			ui.Error(name + " input isnt an integer");
		}
	}
	private void SetPositiveFloat(string name, string input, ref float value)
	{
		try
		{
			float intInput = float.Parse(input);

			if (intInput < 0)
			{
				ui.Error("Invalid " + name);
			}
			else
			{
				value = intInput;
			}
		}
		catch
		{
			ui.Error(name + " input isnt a number");
		}
	}
}
