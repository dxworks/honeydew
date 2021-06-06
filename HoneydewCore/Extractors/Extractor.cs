using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;

namespace HoneydewCore.Extractors
{
    public abstract class Extractor<T> where T : IMetricExtractor
    {
        protected readonly IList<T> MetricExtractors;

        protected Extractor(IList<T> metricExtractors)
        {
            MetricExtractors = metricExtractors ?? new List<T>();
        }

        public abstract string FileType();

        public abstract CompilationUnitModel Extract(string fileContent);
    }
}