using System.Collections.Generic;
using HoneydewCore.Utils;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class AddGenericNamesStrategy : IAddStrategy
{
    private readonly bool _ignorePrimitives;

    public AddGenericNamesStrategy(bool ignorePrimitives)
    {
        _ignorePrimitives = ignorePrimitives;
    }

    public void AddDependency(IDictionary<string, int> dependencies, EntityType type)
    {
        switch (type.Entity)
        {
            case ClassModel classModel:
                AddDependency(dependencies, classModel.Name);
                break;
            case DelegateModel delegateModel:
                AddDependency(dependencies, delegateModel.Name);
                break;
            case InterfaceModel interfaceModel:
                AddDependency(dependencies, interfaceModel.Name);
                break;
        }

        foreach (var containedType in type.GenericTypes)
        {
            AddDependency(dependencies, containedType);
        }
    }

    private void AddDependency(IDictionary<string, int> dependencies, string dependencyName)
    {
        dependencyName = dependencyName.Trim('?');

        if (_ignorePrimitives)
        {
            if (CSharpConstants.IsPrimitive(dependencyName) || CSharpConstants.IsPrimitiveArray(dependencyName))
            {
                return;
            }
        }

        if (dependencies.ContainsKey(dependencyName))
        {
            dependencies[dependencyName]++;
        }
        else
        {
            dependencies.Add(dependencyName, 1);
        }
    }
}
