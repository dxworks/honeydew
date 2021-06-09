using System;
using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Extractors.Models
{
    public class MetricsSet
    {
        public readonly IDictionary<Type, IMetric> Metrics = new Dictionary<Type, IMetric>();

        public int Count => Metrics.Count;

        public bool HasMetrics()
        {
            return Metrics.Count > 0;
        }

        public void AddValue(Type extractorType, IMetric metric)
        {
            if (Metrics.ContainsKey(extractorType))
            {
                return;
            }

            Metrics.Add(extractorType, metric);
        }

        public void Add(IMetricExtractor extractor)
        {
            var type = extractor.GetType();

            AddValue(type, extractor.GetMetric());
        }

        public Optional<IMetric> Get<T>() where T : IMetricExtractor
        {
            return !Metrics.TryGetValue(typeof(T), out var metric) ? default : new Optional<IMetric>(metric);
        }
    }
}