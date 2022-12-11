using DxWorks.ScriptBee.Plugins.Honeydew.Models;

namespace Honeydew.PostExtraction.ReferenceRelations;

public interface IAddStrategy
{
    public void AddDependency(IDictionary<string, int> dependencies, EntityType type);

    public void AddDependency(IDictionary<string, int> dependencies, string typeName, int count);
}
