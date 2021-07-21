using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneydewExtractors.Metrics
{
    public class MetricLoader<TM> : IMetricLoader<TM>
    {
        private readonly ISet<Type> _metricsTypes = new HashSet<Type>();

        public void LoadMetric<T>() where T : TM
        {
            _metricsTypes.Add(typeof(T));
        }

        public IList<TM> GetMetrics()
        {
            return _metricsTypes
                .Select(metricsType => (TM) Activator.CreateInstance(metricsType))
                .ToList();
        }
    }
}
