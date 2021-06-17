using System.Collections.Generic;
using HoneydewCore.Models.Representations;

namespace HoneydewCore.Extractors.Metrics
{
    public interface IRelationMetric
    {
        IList<FileRelation> GetRelations(object metricValue);
    }
}