using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewModels.CSharp;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
{
    public class LocalVariablesRelationVisitor : IModelVisitor<IClassType>, IRelationVisitor
    {
        public string PrettyPrint()
        {
            return "Local Variables Dependency";
        }

        public void Visit(IClassType classType)
        {
            if (classType is not IPropertyMembersClassType classTypeWithProperties)
            {
                return;
            }

            var dependencies = GetDependencies(classTypeWithProperties);

            classTypeWithProperties.Metrics.Add(new MetricModel
            {
                ExtractorName = GetType().ToString(),
                Value = dependencies,
                ValueType = dependencies.GetType().ToString()
            });
        }

        public Dictionary<string, int> GetDependencies(IPropertyMembersClassType classTypeWithProperties)
        {
            var dependencies = new Dictionary<string, int>();

            foreach (var propertyType in classTypeWithProperties.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var localVariableType in accessor.LocalVariableTypes)
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

                    if (accessor is ITypeWithLocalFunctions typeWithLocalFunctions)
                    {
                        ExtractLocalVariablesFromLocalFunctions(typeWithLocalFunctions, dependencies);
                    }
                }
            }

            foreach (var methodType in classTypeWithProperties.Methods)
            {
                foreach (var localVariableType in methodType.LocalVariableTypes)
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

                if (methodType is ITypeWithLocalFunctions typeWithLocalFunctions)
                {
                    ExtractLocalVariablesFromLocalFunctions(typeWithLocalFunctions, dependencies);
                }
            }

            foreach (var constructorType in classTypeWithProperties.Constructors)
            {
                foreach (var localVariableType in constructorType.LocalVariableTypes)
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

                if (constructorType is ITypeWithLocalFunctions typeWithLocalFunctions)
                {
                    ExtractLocalVariablesFromLocalFunctions(typeWithLocalFunctions, dependencies);
                }
            }

            return dependencies;
        }

        private static void ExtractLocalVariablesFromLocalFunctions(ITypeWithLocalFunctions typeWithLocalFunctions,
            IDictionary<string, int> dependencies)
        {
            foreach (var localFunction in typeWithLocalFunctions.LocalFunctions)
            {
                foreach (var localVariableType in localFunction.LocalVariableTypes)
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
}
