using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Models
{
    public class ClassModel : ProjectEntity
    {
        private readonly IDictionary<string, IMetric> _metrics = new Dictionary<string, IMetric>();
        public string Namespace { get; init; }

        public bool HasMetrics()
        {
            return _metrics.Count > 0;
        }

        public void AddMetric(string key, IMetric metric)
        {
            _metrics.Add(key, metric);
        }

        public Optional<Metric<T>> GetMetric<T>(string key)
        {
            if (!_metrics.ContainsKey(key))
            {
                return default;
            }

            var metric = _metrics[key];

            return metric is not Metric<T> metricT ? default : new Optional<Metric<T>>(metricT);
        }
    }
}