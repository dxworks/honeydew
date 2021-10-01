using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
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
                        if (dependencies.ContainsKey(localVariableType.Type.Name))
                        {
                            dependencies[localVariableType.Type.Name]++;
                        }
                        else
                        {
                            dependencies.Add(localVariableType.Type.Name, 1);
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
                    if (dependencies.ContainsKey(localVariableType.Type.Name))
                    {
                        dependencies[localVariableType.Type.Name]++;
                    }
                    else
                    {
                        dependencies.Add(localVariableType.Type.Name, 1);
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
                    if (dependencies.ContainsKey(localVariableType.Type.Name))
                    {
                        dependencies[localVariableType.Type.Name]++;
                    }
                    else
                    {
                        dependencies.Add(localVariableType.Type.Name, 1);
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
                    if (dependencies.ContainsKey(localVariableType.Type.Name))
                    {
                        dependencies[localVariableType.Type.Name]++;
                    }
                    else
                    {
                        dependencies.Add(localVariableType.Type.Name, 1);
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
