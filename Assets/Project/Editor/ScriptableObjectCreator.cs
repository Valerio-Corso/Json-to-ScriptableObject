using System;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
    public static class ScriptableObjectCreator
    {
        public static void CreateAsset(string outputPath, string className)
        {
            var assetType = GetTypeByName(className);
            if (assetType == null)
            {
                Debug.LogError($"Could not find class type {className}. Ensure it is compiled and the name is correct.");
                return;
            }

            var asset = ScriptableObject.CreateInstance(assetType);
            var path = AssetDatabase.GenerateUniqueAssetPath($"{outputPath}/{className}.asset");

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

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