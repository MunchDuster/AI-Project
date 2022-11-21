using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
	public const string extension = ".nn";

	public static bool SaveFile(NeuralNetworkData data)
	{
		BinaryFormatter formatter = new BinaryFormatter();
		string path = GetPath() + data.name + extension;

		FileStream stream = new FileStream(path, FileMode.Create);

		formatter.Serialize(stream, data);

		stream.Close();

		return true;
	}

	public static NeuralNetworkData LoadFile(string filename)
	{
		string path = GetPath() + filename + extension;

		if (File.Exists(path))
		{
			BinaryFormatter formatter = new BinaryFormatter();

			FileStream stream = new FileStream(path, FileMode.Open);

			NeuralNetworkData data = formatter.Deserialize(stream) as NeuralNetworkData;

			stream.Close();

			return data;
		}
		else
		{
			Debug.Log("File not found");
			return null;
		}
	}


	public static string[] GetFileNamesInDirectory(string directory = null)
	{
		string targetDir = (directory == null) ? GetPath() : directory;
		string[] filePaths = Directory.GetFiles(targetDir, "*.nn");

		//Paths -> File Names (with extension)
		for (int i = 0; i < filePaths.Length; i++)
		{
			string[] splits = filePaths[i].Split('/');

			filePaths[i] = splits[splits.Length - 1];
		}

		//Remove Extensions
		for (int i = 0; i < filePaths.Length; i++)
		{
			string[] splits = filePaths[i].Split('.');

			filePaths[i] = splits[0];
		}

		return filePaths;
	}

	public static string GetPath()
	{
		return Application.persistentDataPath + "/";
	}
}