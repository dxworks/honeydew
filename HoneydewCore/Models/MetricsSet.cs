using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Models
{
    public class MetricsSet
    {
        private readonly ISet<IMetric> _metrics = new HashSet<IMetric>();

        public bool HasMetrics()
        {
            return _metrics.Count > 0;
        }

        public void Add(IMetric metric)
        {
            _metrics.Add(metric);
        }

        public Optional<Metric<T>> Get<T>()
        {
            var metric = _metrics.FirstOrDefault(m => m.GetType() == typeof(Metric<T>));
            return metric == default ? default : new Optional<Metric<T>>((Metric<T>) metric);
        }
    }
}