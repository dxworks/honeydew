using System.Collections.Generic;
using HoneydewModels.Reference;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.ReferenceRelations
{
    public interface IAddStrategy
    {
        public void AddDependency(IDictionary<string, int> dependencies, EntityType type);
    }
}
