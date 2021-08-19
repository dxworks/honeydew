using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public interface IRelationMetricHolder
    {
        void Add(string className, string dependencyName);

        public IDictionary<string, int> GetDependencies(string className);
        
        public IList<FileRelation> GetRelations(IDictionary<string, IDictionary<string, int>> dependencies);
    }
}
