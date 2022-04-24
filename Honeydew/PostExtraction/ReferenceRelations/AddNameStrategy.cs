using System.Collections.Generic;
using Honeydew.ScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public class AddNameStrategy : IAddStrategy
{
    public virtual void AddDependency(IDictionary<string, int> dependencies, EntityType type)
    {
        // var dependencyName = type.Entity.Name.Trim('?'); todo ask someone
        IAddStrategy addStrategy = this;
        addStrategy.AddDependency(dependencies, type.Name, 1);
    }

    public virtual void AddDependency(IDictionary<string, int> dependencies, string typeName, int count)
    {
        var dependencyName = typeName.Trim('?');
        if (dependencies.ContainsKey(dependencyName))
        {
            dependencies[dependencyName] += count;
        }
        else
        {
            dependencies.Add(dependencyName, count);
        }
    }
}
