using System.Collections.Generic;
using HoneydewScriptBeePlugin.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public interface IAddStrategy
{
    public void AddDependency(IDictionary<string, int> dependencies, EntityType type);
}
