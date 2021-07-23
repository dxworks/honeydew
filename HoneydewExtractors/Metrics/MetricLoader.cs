using System;
using System.Collections.Generic;
using System.Linq;

namespace HoneydewExtractors.Metrics
{
    public class MetricLoader<TM>
    {
        private readonly ISet<Type> _metricsTypes = new HashSet<Type>();

        public void LoadMetric<T>() where T : TM
        {
            _metricsTypes.Add(typeof(T));
        }

        public void LoadMetric(Type type)
        {
            if (!typeof(TM).IsAssignableFrom(type))
            {
                return;
            }

            _metricsTypes.Add(type);
        }

        public IList<TM> InstantiateMetrics()
        {
            return _metricsTypes
                .Select(metricsType => (TM) Activator.CreateInstance(metricsType))
                .ToList();
        }
    }
}
