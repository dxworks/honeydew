using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class LocalVariablesRelationVisitor : IReferenceModelVisitor
{
    public const string LocalVariablesDependencyMetricName = "LocalVariablesDependency";

    private readonly IAddStrategy _addStrategy;

    public LocalVariablesRelationVisitor(IAddStrategy addStrategy)
    {
        _addStrategy = addStrategy;
    }

    public void Visit(ReferenceEntity referenceEntity)
    {
        if (referenceEntity is not EntityModel entityModel)
        {
            return;
        }

        entityModel[LocalVariablesDependencyMetricName] = Visit(entityModel);
    }

    private Dictionary<string, int> Visit(EntityModel entityModel)
    {
        var dependencies = new Dictionary<string, int>();

        if (entityModel is ClassModel classModel)
        {
            foreach (var propertyType in classModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var localVariableType in accessor.LocalVariables)
                    {
                        _addStrategy.AddDependency(dependencies, localVariableType.Type);
                    }

                    foreach (var localFunction in accessor.LocalFunctions)
                    {
                        ExtractLocalVariablesFromLocalFunctions(dependencies, localFunction);
                    }
                }
            }

            foreach (var methodType in classModel.Methods)
            {
                foreach (var localVariableType in methodType.LocalVariables)
                {
                    _addStrategy.AddDependency(dependencies, localVariableType.Type);
                }

                foreach (var localFunction in methodType.LocalFunctions)
                {
                    ExtractLocalVariablesFromLocalFunctions(dependencies, localFunction);
                }
            }

            foreach (var methodType in classModel.Constructors)
            {
                foreach (var localVariableType in methodType.LocalVariables)
                {
                    _addStrategy.AddDependency(dependencies, localVariableType.Type);
                }

                foreach (var localFunction in methodType.LocalFunctions)
                {
                    ExtractLocalVariablesFromLocalFunctions(dependencies, localFunction);
                }
            }

            if (classModel.Destructor != null)
            {
                foreach (var localVariableType in classModel.Destructor.LocalVariables)
                {
                    _addStrategy.AddDependency(dependencies, localVariableType.Type);
                }

                foreach (var localFunction in classModel.Destructor.LocalFunctions)
                {
                    ExtractLocalVariablesFromLocalFunctions(dependencies, localFunction);
                }
            }
        }
        else if (entityModel is InterfaceModel interfaceModel)
        {
            foreach (var propertyType in interfaceModel.Properties)
            {
                foreach (var accessor in propertyType.Accessors)
                {
                    foreach (var localVariableType in accessor.LocalVariables)
                    {
                        _addStrategy.AddDependency(dependencies, localVariableType.Type);
                    }

                    foreach (var localFunction in accessor.LocalFunctions)
                    {
                        ExtractLocalVariablesFromLocalFunctions(dependencies, localFunction);
                    }
                }
            }

            foreach (var methodType in interfaceModel.Methods)
            {
                foreach (var localVariableType in methodType.LocalVariables)
                {
                    _addStrategy.AddDependency(dependencies, localVariableType.Type);
                }

                foreach (var localFunction in methodType.LocalFunctions)
                {
                    ExtractLocalVariablesFromLocalFunctions(dependencies, localFunction);
                }
            }
        }

        return dependencies;
    }

    private void ExtractLocalVariablesFromLocalFunctions(IDictionary<string, int> dependencies,
        MethodModel typeWithLocalFunctions)
    {
        foreach (var localFunction in typeWithLocalFunctions.LocalFunctions)
        {
            foreach (var localVariableType in localFunction.LocalVariables)
            {
                _addStrategy.AddDependency(dependencies, localVariableType.Type);
            }

            foreach (var innerLocalFunction in localFunction.LocalFunctions)
            {
                ExtractLocalVariablesFromLocalFunctions(dependencies, innerLocalFunction);
            }
        }
    }
}
