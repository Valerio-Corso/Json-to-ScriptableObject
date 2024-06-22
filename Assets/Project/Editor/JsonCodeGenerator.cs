using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Linq;

public static class JsonCodeGenerator
{
    public static void GenerateJsonClasses(JsonCodegenObject codegenObj)
    {
        var jObject = JObject.Parse(codegenObj.Json);
        var classDefinitions = new Dictionary<string, StringBuilder>();
        codegenObj.JsonRootClassName = $"{codegenObj.JsonName}Root";
        GenerateClasses(codegenObj.JsonRootClassName, jObject, classDefinitions);

        foreach (var classDef in classDefinitions)
        {
            var filePath = Path.Combine(codegenObj.OutputPath, $"{classDef.Key}.cs");
            File.WriteAllText(filePath, classDef.Value.ToString());
        }
    }
    
    public static void GenerateScriptableRootClass(JsonCodegenObject codegenObject, string outputPath)
    {
        var classBuilder = new StringBuilder();
        classBuilder.AppendLine("using UnityEngine;");
        classBuilder.AppendLine("[CreateAssetMenu(fileName = \"GameData\", menuName = \"ScriptableObjects/GameData\", order = 1)]");
        classBuilder.AppendLine($"public class {codegenObject.JsonName}ScriptableObject : ScriptableObject");
        classBuilder.AppendLine("{");
        classBuilder.AppendLine($"    public {codegenObject.JsonRootClassName} Data;");
        classBuilder.AppendLine("}");

        var className = $"{codegenObject.JsonName}ScriptableObject";
        var fileName = $"{className}.cs";
        string filePath = Path.Combine(outputPath, fileName);
        File.WriteAllText(filePath, classBuilder.ToString());
        codegenObject.ScriptableObjectClassName = className;
    }
    
    private static void GenerateClasses(string className, JObject jObject, IDictionary<string, StringBuilder> classDefinitions)
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
                GenerateClasses(propType, (JObject)propValue, classDefinitions);
            }
            else if (propValue.Type == JTokenType.Array)
            {
                var array = (JArray)propValue;
                if (array.Count > 0 && array[0].Type == JTokenType.Object)
                {
                    propType = ToPascalCase(propName) + "[]";
                    GenerateClasses(ToPascalCase(propName), (JObject)array[0], classDefinitions);
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
        return char.ToUpper(str[0]) + str[1..];
    }
}
