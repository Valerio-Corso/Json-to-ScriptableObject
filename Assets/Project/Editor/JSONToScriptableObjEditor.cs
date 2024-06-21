using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using Project.Editor;
using UnityEditor.Compilation;

public class JSONToScriptableObjEditor : EditorWindow
{
    private string outputPath = "Assets/Content/Sample";
    
    [MenuItem("Tools/Scriptable Object Maker")]
    public static void ShowWindow()
    {
        GetWindow<JSONToScriptableObjEditor>("GameData Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("GameData Editor", EditorStyles.boldLabel);
        // jsonFilePath = EditorGUILayout.TextField("JSON File Path", jsonFilePath);
        // outputPath = EditorGUILayout.TextField("Output Path", outputPath);

        if (GUILayout.Button("Generate C# Classes"))
        {
            GenerateCSharpClasses();
        }
    }

    private void GenerateCSharpClasses()
    {
        if (TryPickFilePath(out var jsonFilePath)) return;

        var json = File.ReadAllText(jsonFilePath);
        JsonCodeGenerator.GenerateClasses(json, outputPath);
        Debug.Log("C# classes generated successfully!");
        JsonCodeGenerator.GenerateScriptableRootClass(outputPath, "Root");
        
        AssetDatabase.Refresh();
        
        // We must compile the just generated scripts, subscribe to the OnFinished event
        _pendingFileCreation = true;
    }

    private bool _pendingFileCreation;

    private void Update()
    {
        if (!EditorApplication.isCompiling && _pendingFileCreation)
        {
            _pendingFileCreation = false;
            Debug.Log("Creating asset");
            ScriptableObjectCreator.CreateAsset(outputPath, "RootScriptableObject");
        }
    }
    
    private static bool TryPickFilePath(out string jsonFilePath)
    {
        jsonFilePath = EditorUtility.OpenFilePanel("Select a file", Application.dataPath, "json");
        if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
        {
            Debug.LogError("Invalid JSON file path");
            return true;
        }

        return false;
    }
}
