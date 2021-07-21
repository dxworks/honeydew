using System.Collections.Generic;

namespace HoneydewExtractors.Metrics
{
    public interface IMetricLoader<TM>
    {
        void LoadMetric<T>() where T : TM;

        IList<TM> GetMetrics();
    }
}
