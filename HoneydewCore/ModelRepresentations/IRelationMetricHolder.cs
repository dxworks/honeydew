using System.Collections.Generic;

namespace HoneydewCore.ModelRepresentations
{
    public interface IRelationMetricHolder
    {
        void Add(string className, string dependencyName, IRelationMetric relationMetric);

        IDictionary<IRelationMetric, IDictionary<string, int>> GetDependencies(string className);

        IList<FileRelation> GetRelations();
    }
}
