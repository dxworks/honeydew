using System.Collections.Generic;
using HoneydewModels.Representations;

namespace HoneydewExtractors.Metrics
{
    public interface IRelationMetric
    {
        IList<FileRelation> GetRelations(object metricValue);
    }
}
