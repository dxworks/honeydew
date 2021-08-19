using System;
using System.Collections.Generic;
using HoneydewCore.ModelRepresentations;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.Core.Metrics
{
    public class MetricRelationsProvider : IMetricRelationsProvider
    {
        public IList<FileRelation> GetFileRelations(MetricModel metricModel)
        {
            if (string.IsNullOrWhiteSpace(metricModel.ExtractorName))
            {
                return new List<FileRelation>();
            }

            var extractorType = Type.GetType(metricModel.ExtractorName);
            if (extractorType == null || !typeof(IRelationMetric).IsAssignableFrom(extractorType))
            {
                return new List<FileRelation>();
            }

            var relationMetric = (IRelationMetric)Activator.CreateInstance(extractorType);

            return relationMetric == null
                ? new List<FileRelation>()
                : relationMetric.GetRelations((IDictionary<string, IDictionary<string, int>>)metricModel.Value);
        }
    }
}
