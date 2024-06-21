using UnityEngine;
using UnityEditor;
using System.IO;

public class JSONToScriptableObjEditor : EditorWindow
{
    private string jsonFilePath = "Assets/Content/Sample/sample.json";
    private string outputPath = "Assets/Content/Sample";
    
    [MenuItem("Tools/Scriptable Object Maker")]
    public static void ShowWindow()
    {
        GetWindow<JSONToScriptableObjEditor>("GameData Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("GameData Editor", EditorStyles.boldLabel);

        TextAsset p = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonFilePath);
        TextAsset ph = AssetDatabase.LoadAssetAtPath<TextAsset>("Content/Sample/sample.json");

        // jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);
        // outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        if (GUILayout.Button("Generate C# Classes"))
        {
            GenerateCSharpClasses();
        }

        // if (GUILayout.Button("Create ScriptableObject"))
        // {
        //     CreateScriptableObjectFromJson();
        // }
    }

    private void GenerateCSharpClasses()
    {
        if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
        {
            Debug.LogError("Invalid JSON file path");
            return;
        }

        string json = File.ReadAllText(jsonFilePath);
        JsonCodeGenerator.GenerateClasses(json, outputPath);
        JsonCodeGenerator.GenerateScriptableObjectClass(outputPath, "Root");

        AssetDatabase.Refresh();
        Debug.Log("C# classes generated successfully!");
    }

    // private void CreateScriptableObjectFromJson()
    // {
    //     if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
    //     {
    //         Debug.LogError("Invalid JSON file path");
    //         return;
    //     }
    //
    //     string json = File.ReadAllText(jsonFilePath);
    //     var gameData = JsonCodeGenerator.LoadJson<RootScriptableObject>(jsonFilePath);
    //     if (gameData != null)
    //     {
    //         string assetPath = "Assets/GameData.asset";
    //         AssetDatabase.CreateAsset(gameData, assetPath);
    //         AssetDatabase.SaveAssets();
    //         AssetDatabase.Refresh();
    //         EditorUtility.FocusProjectWindow();
    //         Selection.activeObject = gameData;
    //
    //         Debug.Log("ScriptableObject created successfully!");
    //     }
    //     else
    //     {
    //         Debug.LogError("Failed to create ScriptableObject");
    //     }
    // }
}
