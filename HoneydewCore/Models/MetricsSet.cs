using System;
using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Models
{
    public class MetricsSet
    {
        public Dictionary<string, object> MetricValues { get; } = new();

        private readonly IDictionary<Type, IMetric> _metrics = new Dictionary<Type, IMetric>();

        public bool HasMetrics()
        {
            return _metrics.Count > 0;
        }

        public void Add(IMetricExtractor extractor)
        {
            var type = extractor.GetType();

            if (_metrics.ContainsKey(type))
            {
                return;
            }

            var metric = extractor.GetMetric();

            _metrics.Add(type, metric);

            MetricValues.Add(type.ToString(), metric);
        }

        public Optional<IMetric> Get<T>() where T : IMetricExtractor
        {
            return !_metrics.TryGetValue(typeof(T), out var metric) ? default : new Optional<IMetric>(metric);
        }
    }
}