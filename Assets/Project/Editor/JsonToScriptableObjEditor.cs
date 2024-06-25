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
        CodeGeneratorFromJson.GenerateJsonClasses(codegenObj);
        Debug.Log("C# classes generated successfully!");
        CodeGeneratorFromJson.GenerateScriptableRootClass(codegenObj, outputPath);
        
        AssetDatabase.Refresh();
        _pendingCodegenObject = codegenObj;
    }

    private static JsonCodegenObject _pendingCodegenObject;
    private void Update()
    {
        CheckForPendingFileCreation();
    }

    private void CheckForPendingFileCreation()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || _pendingCodegenObject == null) return;
        
        Debug.Log("Pending scriptable object creation true...");
        try
        {
            ScriptableObjectCreator.CreateAsset(_pendingCodegenObject);
            ScriptableObjectCreator.PopulateAsset(_pendingCodegenObject);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create ScriptableObject: {e.Message}");
        }
        finally
        {
            _pendingCodegenObject = null;
        }
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
