using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace Project.Editor.Codegen
{
    public class CodeGeneratorFromJson
    {
        private readonly JsonCodegenObject _codegenObject;

        public CodeGeneratorFromJson(JsonCodegenObject codegenObject)
        {
            _codegenObject = codegenObject;
        }
    
        public void GenerateFolderStructure()
        {
            var rootDirectory = Path.Combine(_codegenObject.OutputPath, "Generated");
            var directory = Path.Combine(rootDirectory, _codegenObject.JsonName);
            Directory.CreateDirectory(rootDirectory);
            Directory.CreateDirectory(directory);
            _codegenObject.OutputPath = directory;
        }
    
        public void GenerateJsonClasses()
        {
            var jObject = JObject.Parse(_codegenObject.Json);
            var classDefinitions = new Dictionary<string, StringBuilder>();
            _codegenObject.JsonRootClassName = $"{_codegenObject.JsonName}Root";
            GenerateClasses(_codegenObject.JsonRootClassName, jObject, classDefinitions);

            foreach (var classDef in classDefinitions)
            {
                var filePath = Path.Combine(_codegenObject.OutputPath, $"{_codegenObject.JsonName}{classDef.Key}.cs");
                File.WriteAllText(filePath, classDef.Value.ToString());
            }
        }
    
        public void GenerateScriptableRootClass()
        {
            var classBuilder = new StringBuilder();
            using (var writer = new IndentedTextWriter(new StringWriter(classBuilder), "    "))
            {
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine($"using JsonToCSharp.Generated.{_codegenObject.JsonName};");
                writer.WriteLine("[CreateAssetMenu(fileName = \"GameData\", menuName = \"ScriptableObjects/GameData\", order = 1)]");
                writer.WriteLine($"public class {_codegenObject.JsonName}ScriptableObject : ScriptableObject");
                writer.WriteLine("{");
                writer.Indent++;
                writer.WriteLine($"public {_codegenObject.JsonRootClassName} Data;");
                writer.Indent--;
                writer.WriteLine("}");
            }

            var className = $"{_codegenObject.JsonName}ScriptableObject";
            var fileName = $"{className}.cs";
            string filePath = Path.Combine(_codegenObject.OutputPath, fileName);
            File.WriteAllText(filePath, classBuilder.ToString());
            _codegenObject.ScriptableObjectClassName = className;
        }
    
        private void GenerateClasses(string className, JObject jObject, IDictionary<string, StringBuilder> classDefinitions)
        {
            var classBuilder = new StringBuilder();
            using (var writer = new IndentedTextWriter(new StringWriter(classBuilder), "    "))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine();
                writer.WriteLine($"namespace JsonToCSharp.Generated.{_codegenObject.JsonName}");
                writer.WriteLine(CodegenFormatterHelper.GetBraceParentheses(true));
                writer.Indent++;
                writer.WriteLine("[System.Serializable]");
                writer.WriteLine($"public class {className}");
                writer.WriteLine(CodegenFormatterHelper.GetBraceParentheses(true));
                writer.Indent++;

                foreach (var property in jObject.Properties())
                {
                    string propName = property.Name;
                    JToken propValue = property.Value;

                    string propType;
                    if (propValue.Type == JTokenType.Object)
                    {
                        propType = CodegenFormatterHelper.ToPascalCase(propName);
                        GenerateClasses(propType, (JObject)propValue, classDefinitions);
                    }
                    else if (propValue.Type == JTokenType.Array)
                    {
                        var array = (JArray)propValue;
                        if (array.Count > 0 && array[0].Type == JTokenType.Object)
                        {
                            propType = $"List<{CodegenFormatterHelper.ToPascalCase(propName)}>";
                            GenerateClasses(CodegenFormatterHelper.ToPascalCase(propName), (JObject)array[0], classDefinitions);
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

                    writer.WriteLine($"public {propType} {CodegenFormatterHelper.ToPascalCase(propName)};");
                }

                writer.Indent--;
                writer.WriteLine(CodegenFormatterHelper.GetBraceParentheses(false));
                writer.Indent--;
                writer.WriteLine(CodegenFormatterHelper.GetBraceParentheses(false));
            }

            classDefinitions[className] = classBuilder;
        }

        private string GetCSharpType(JTokenType jsonType)
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
    }
}
