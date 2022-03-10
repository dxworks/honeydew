using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class AddNameStrategy : IAddStrategy
{
    public void AddDependency(IDictionary<string, int> dependencies, EntityType type)
    {
        // var dependencyName = type.Entity.Name.Trim('?'); todo ask someone
        var dependencyName = type.Name.Trim('?');
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
