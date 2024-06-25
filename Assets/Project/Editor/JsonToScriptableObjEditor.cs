using UnityEngine;
using UnityEditor;
using System.IO;
using Project.Editor;

public class JsonCodegenObject
{
    public string Json;
    public string JsonName;
    public string ScriptableObjectClassName;
    public string JsonRootClassName;
    public string Path;
    public string OutputPath;
    public ScriptableObject Asset;
}

public class JsonToScriptableObjEditor : EditorWindow
{
    private string outputPath = "Assets/Content/Sample";

    [MenuItem("Tools/Scriptable Object Maker")]
    public static void ShowWindow()
    {
        GetWindow<JsonToScriptableObjEditor>("GameData Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("GameData Editor", EditorStyles.boldLabel);
        if (GUILayout.Button("Generate C# Classes"))
        {
            GenerateCSharpClasses();
        }
    }

    private void GenerateCSharpClasses()
    {
        var codegenObj = new JsonCodegenObject { OutputPath = outputPath };
        if (TryPickFilePath(codegenObj)) return;
        
        codegenObj.Json = File.ReadAllText(codegenObj.Path);

        var codeGenerator = new CodeGeneratorFromJson(codegenObj);
        codeGenerator.GenerateFolderStructure();
        codeGenerator.GenerateJsonClasses();
        codeGenerator.GenerateScriptableRootClass();
        AssetDatabase.Refresh();
        SaveDataForScriptableObjectCreation(codegenObj);
    }

    private static void SaveDataForScriptableObjectCreation(JsonCodegenObject codegenObj)
    {
        EditorPrefs.SetBool("PendingScriptableCreation", true);
        EditorPrefs.SetString("PendingScriptablePath", codegenObj.OutputPath);
        EditorPrefs.SetString("PendingJsonFilePath", codegenObj.Path);
        EditorPrefs.SetString("PendingScriptableObjectClassName", codegenObj.ScriptableObjectClassName);
    }

    private static bool TryPickFilePath(JsonCodegenObject codegenObject)
    {
        var jsonFilePath = EditorUtility.OpenFilePanel("Select a file", Application.dataPath, "json");
        if (string.IsNullOrEmpty(jsonFilePath) || !File.Exists(jsonFilePath))
        {
            Debug.LogError("Invalid JSON file path");
            return true;
        }
        
        codegenObject.JsonName = CodegenStringHelper.ToPascalCase(Path.GetFileNameWithoutExtension(jsonFilePath));
        codegenObject.Path = jsonFilePath;

        return false;
    }
}
