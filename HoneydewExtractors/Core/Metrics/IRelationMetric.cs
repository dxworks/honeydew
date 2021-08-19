using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;

namespace HoneydewExtractors.Core.Metrics
{
    public interface IRelationMetric
    {
        string PrettyPrint();
        
        IList<FileRelation> GetRelations(IDictionary<string, IDictionary<string, int>> dependencies);
    }
}
