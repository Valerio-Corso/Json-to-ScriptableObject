using System.IO;
using Project.Editor;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ScriptableObjectCreationHandler
{
	static ScriptableObjectCreationHandler()
	{
		CheckForPendingFileCreation();
	}

	private static void CheckForPendingFileCreation()
	{
		if (EditorPrefs.GetBool("PendingScriptableCreation", false))
		{
			EditorPrefs.SetBool("PendingScriptableCreation", false);
			var outputPath = EditorPrefs.GetString("PendingScriptablePath");
			var jsonFilePath = EditorPrefs.GetString("PendingJsonFilePath");
			var scriptableObjectClassName = EditorPrefs.GetString("PendingScriptableObjectClassName");

			if (!File.Exists(jsonFilePath))
			{
				Debug.LogError("JSON file path no longer exists.");
				CleanUp();
				return;
			}

			var codegenObj = new JsonCodegenObject
			{
					OutputPath = outputPath,
					Path = jsonFilePath,
					Json = File.ReadAllText(jsonFilePath),
					ScriptableObjectClassName = scriptableObjectClassName
			};

			try
			{
				ScriptableObjectCreator.CreateOrUpdateAsset(codegenObj);
				Debug.Log("ScriptableObject creation or update completed.");
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Failed to create or update ScriptableObject: {e.Message}");
			}
			finally
			{
				CleanUp();
			}
		}
	}

	private static void CleanUp()
	{
		EditorPrefs.DeleteKey("PendingScriptableCreation");
		EditorPrefs.DeleteKey("PendingScriptablePath");
		EditorPrefs.DeleteKey("PendingJsonFilePath");
		EditorPrefs.DeleteKey("PendingScriptableObjectClassName");
	}
}