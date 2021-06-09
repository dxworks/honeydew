using System;
using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Models
{
    public class MetricsSet
    {
        private readonly IDictionary<Type, IMetric> _metrics = new Dictionary<Type, IMetric>();

        public bool HasMetrics()
        {
            return _metrics.Count > 0;
        }

        public void Add(IMetricExtractor extractor)
        {
            if (!_metrics.ContainsKey(extractor.GetType()))
            {
                _metrics.Add(extractor.GetType(), extractor.GetMetric());
            }
        }

        public Optional<IMetric> Get<T>() where T : IMetricExtractor
        {
            return !_metrics.TryGetValue(typeof(T), out var metric) ? default : new Optional<IMetric>(metric);
        }
    }
}