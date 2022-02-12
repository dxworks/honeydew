using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public class AddNameStrategy : IAddStrategy
    {
        public void AddDependency(IDictionary<string, int> dependencies, EntityType type)
        {
            var dependencyName = type.Name;
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
}
