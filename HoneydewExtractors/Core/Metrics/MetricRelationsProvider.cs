using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.Core.Metrics
{
    public class MetricRelationsProvider : IMetricRelationsProvider
    {
        public IList<FileRelation> GetFileRelations(ClassMetric classMetric)
        {
            if (string.IsNullOrWhiteSpace(classMetric.ExtractorName))
            {
                return new List<FileRelation>();
            }

            var extractorType = Type.GetType(classMetric.ExtractorName);
            if (extractorType == null || !typeof(IRelationMetric).IsAssignableFrom(extractorType))
            {
                return new List<FileRelation>();
            }

            var relationMetric = (IRelationMetric) Activator.CreateInstance(extractorType);

            return relationMetric == null ? new List<FileRelation>() : relationMetric.GetRelations(classMetric.Value);
        }
    }
}
