using System;
using HoneydewCore.ModelRepresentations;
using HoneydewExtractors.Core.Metrics;

namespace HoneydewExtractors.Core
{
    public class MetricPrettier : IMetricPrettier
    {
        public string Pretty(string metricFullName)
        {
            if (string.IsNullOrWhiteSpace(metricFullName))
            {
                return "";
            }

            var type = Type.GetType(metricFullName);
            if (type == null || !typeof(IMetric).IsAssignableFrom(type))
            {
                return metricFullName;
            }

            var metricExtractor = (IMetric) Activator.CreateInstance(type);

            return metricExtractor == null ? metricFullName : metricExtractor.PrettyPrint();
        }
    }
}
