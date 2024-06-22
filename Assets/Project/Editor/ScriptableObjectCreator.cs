using System;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
    public static class ScriptableObjectCreator
    {
        public static void CreateAsset(JsonCodegenObject codegenObject)
        {
            var assetType = GetTypeByName(codegenObject.ScriptableObjectClassName);
            if (assetType == null)
            {
                Debug.LogError($"Could not find class type {codegenObject.JsonRootClassName}. Ensure it is compiled and the name is correct.");
                return;
            }

            codegenObject.Asset = ScriptableObject.CreateInstance(assetType);
            var path = $"{codegenObject.OutputPath}/{codegenObject.ScriptableObjectClassName}.asset";

            // Check if the asset already exists
            var existingAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (existingAsset != null)
            {
                AssetDatabase.DeleteAsset(path);
            }

            AssetDatabase.CreateAsset(codegenObject.Asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static void PopulateAsset(JsonCodegenObject codegenObject)
        {
            var type = codegenObject.Asset.GetType();
            var fieldInfo = type.GetField("Data");
            var fieldType = fieldInfo.FieldType;
            var data = JsonUtility.FromJson(codegenObject.Json, fieldType);
            fieldInfo.SetValue(codegenObject.Asset, data);
        }

        private static void SelectObject(ScriptableObject asset)
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
    }
}