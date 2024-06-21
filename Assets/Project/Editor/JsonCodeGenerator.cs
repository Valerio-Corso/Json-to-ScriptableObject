using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Linq;

public static class JsonCodeGenerator
{
    public static void GenerateClasses(string json, string outputPath)
    {
        var jObject = JObject.Parse(json);
        var classDefinitions = new Dictionary<string, StringBuilder>();
        GenerateClass("Root", jObject, classDefinitions);

        foreach (var classDef in classDefinitions)
        {
            var filePath = Path.Combine(outputPath, $"{classDef.Key}.cs");
            File.WriteAllText(filePath, classDef.Value.ToString());
        }
    }
    
    public static void GenerateScriptableRootClass(string outputPath, string rootClassName)
    {
        var classBuilder = new StringBuilder();
        classBuilder.AppendLine("using UnityEngine;");
        classBuilder.AppendLine("[CreateAssetMenu(fileName = \"GameData\", menuName = \"ScriptableObjects/GameData\", order = 1)]");
        classBuilder.AppendLine($"public class {rootClassName}ScriptableObject : ScriptableObject");
        classBuilder.AppendLine("{");
        classBuilder.AppendLine($"    public {rootClassName} Data;");
        classBuilder.AppendLine("}");

        string filePath = Path.Combine(outputPath, $"{rootClassName}ScriptableObject.cs");
        File.WriteAllText(filePath, classBuilder.ToString());
    }
    
    private static void GenerateClass(string className, JObject jObject, IDictionary<string, StringBuilder> classDefinitions)
    {
        var classBuilder = new StringBuilder();
        classBuilder.AppendLine("using System;");
        classBuilder.AppendLine("using UnityEngine;");
        classBuilder.AppendLine("[System.Serializable]");
        classBuilder.AppendLine($"public class {className}");
        classBuilder.AppendLine("{");

        foreach (var property in jObject.Properties())
        {
            string propName = property.Name;
            JToken propValue = property.Value;

            string propType;
            if (propValue.Type == JTokenType.Object)
            {
                propType = ToPascalCase(propName);
                GenerateClass(propType, (JObject)propValue, classDefinitions);
            }
            else if (propValue.Type == JTokenType.Array)
            {
                var array = (JArray)propValue;
                if (array.Count > 0 && array[0].Type == JTokenType.Object)
                {
                    propType = ToPascalCase(propName) + "[]";
                    GenerateClass(ToPascalCase(propName), (JObject)array[0], classDefinitions);
                }
                else
                {
                    propType = "object[]";
                }
            }
            else
            {
                propType = GetCSharpType(propValue.Type);
            }

            classBuilder.AppendLine($"    public {propType} {ToPascalCase(propName)};");
        }

        classBuilder.AppendLine("}");
        classDefinitions[className] = classBuilder;
    }

    private static string GetCSharpType(JTokenType jsonType)
    {
        switch (jsonType)
        {
            case JTokenType.Integer:
                return "int";
            case JTokenType.Float:
                return "float";
            case JTokenType.String:
                return "string";
            case JTokenType.Boolean:
                return "bool";
            default:
                return "string";
        }
    }

    private static string ToPascalCase(string str)
    {
        return char.ToUpper(str[0]) + str.Substring(1);
    }
}
