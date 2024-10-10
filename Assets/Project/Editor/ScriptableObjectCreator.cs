using System;
using Project.Editor.Codegen;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
    public static class ScriptableObjectCreator
    {
        public static void CreateOrUpdateAsset(JsonCodegenObject codegenObject)
        {
            var assetType = GetTypeByName(codegenObject.ScriptableObjectClassName);
            if (assetType == null)
            {
                Debug.LogError($"Could not find class type {codegenObject.JsonRootClassName}. Ensure it is compiled and the name is correct.");
                return;
            }

            var path = $"{codegenObject.OutputPath}/{codegenObject.ScriptableObjectClassName}.asset";

            ScriptableObject asset;
            if (AssetExists(path))
            {
                asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                Debug.Log($"Asset already exists at path: {path}. Updating existing asset.");
            }
            else
            {
                asset = ScriptableObject.CreateInstance(assetType);
                AssetDatabase.CreateAsset(asset, path);
                Debug.Log($"Created new asset at path: {path}.");
            }

            codegenObject.Asset = asset;
            PopulateAsset(codegenObject);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            SelectObject(asset);
            
        }

        private static void PopulateAsset(JsonCodegenObject codegenObject)
        {
            var type = codegenObject.Asset.GetType();
            var fieldInfo = type.GetField("Data");
            var fieldType = fieldInfo.FieldType;
            var data = JsonUtility.FromJson(codegenObject.Json, fieldType);
            fieldInfo.SetValue(codegenObject.Asset, data);
        }

        public static void SelectObject(ScriptableObject asset)
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        private static Type GetTypeByName(string className)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(className);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }

        private static bool AssetExists(string path)
        {
            return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null;
        }
    }
}
