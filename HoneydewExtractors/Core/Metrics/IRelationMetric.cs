using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;

namespace HoneydewExtractors.Core.Metrics
{
    public interface IRelationMetric
    {
        IList<FileRelation> GetRelations(object metricValue);
    }
}
