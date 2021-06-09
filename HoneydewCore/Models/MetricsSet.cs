using System;
using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Models
{
    public class MetricsSet
    {
        public Dictionary<string, object> MetricValues { get; } = new();

        public readonly IDictionary<Type, IMetric> Metrics = new Dictionary<Type, IMetric>();

        public bool HasMetrics()
        {
            return Metrics.Count > 0;
        }

        public void Add(IMetricExtractor extractor)
        {
            var type = extractor.GetType();

            if (Metrics.ContainsKey(type))
            {
                return;
            }

            var metric = extractor.GetMetric();

            Metrics.Add(type, metric);

            MetricValues.Add(type.ToString(), metric);
        }

        public Optional<IMetric> Get<T>() where T : IMetricExtractor
        {
            return !Metrics.TryGetValue(typeof(T), out var metric) ? default : new Optional<IMetric>(metric);
        }
    }
}