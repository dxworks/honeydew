using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class AddGenericNamesStrategy : IAddStrategy
{
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

    private static void AddDependency(IDictionary<string, int> dependencies, string dependencyName)
    {
        dependencyName = dependencyName.Trim('?');
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
