using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

public class LocalVariablesRelationVisitor : IModelVisitor<ClassModel>, IRelationVisitor
{
    public string PrettyPrint()
    {
        return "Local Variables Dependency";
    }

    public void Visit(ClassModel classModel)
    {
        var dependencies = GetDependencies(classModel);

        classModel.Metrics.Add(new MetricModel
        {
            ExtractorName = GetType().ToString(),
            Value = dependencies,
            ValueType = dependencies.GetType().ToString()
        });
    }

    public Dictionary<string, int> GetDependencies(ClassModel classModel)
    {
        var dependencies = new Dictionary<string, int>();

        foreach (var propertyType in classModel.Properties)
        {
            foreach (var accessor in propertyType.Accessors)
            {
                foreach (var localVariableType in accessor.LocalVariables)
                {
                    var typeName = CSharpConstants.GetNonNullableName(localVariableType.Type.Name);
                    if (dependencies.ContainsKey(typeName))
                    {
                        dependencies[typeName]++;
                    }
                    else
                    {
                        dependencies.Add(typeName, 1);
                    }
                }

                ExtractLocalVariablesFromLocalFunctions(accessor, dependencies);
            }
        }

        foreach (var methodModel in classModel.Methods)
        {
            foreach (var localVariableType in methodModel.LocalVariables)
            {
                var typeName = CSharpConstants.GetNonNullableName(localVariableType.Type.Name);
                if (dependencies.ContainsKey(typeName))
                {
                    dependencies[typeName]++;
                }
                else
                {
                    dependencies.Add(typeName, 1);
                }
            }

            ExtractLocalVariablesFromLocalFunctions(methodModel, dependencies);
        }

        return dependencies;
    }

    private static void ExtractLocalVariablesFromLocalFunctions(MethodModel methodModel,
        IDictionary<string, int> dependencies)
    {
        foreach (var localFunction in methodModel.LocalFunctions)
        {
            foreach (var localVariableType in localFunction.LocalVariables)
            {
                var typeName = CSharpConstants.GetNonNullableName(localVariableType.Type.Name);
                if (dependencies.ContainsKey(typeName))
                {
                    dependencies[typeName]++;
                }
                else
                {
                    dependencies.Add(typeName, 1);
                }
            }

            foreach (var innerLocalFunction in localFunction.LocalFunctions)
            {
                ExtractLocalVariablesFromLocalFunctions(innerLocalFunction, dependencies);
            }
        }
    }
}
