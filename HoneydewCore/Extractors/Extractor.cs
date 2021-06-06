using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Models;

namespace HoneydewCore.Extractors
{
    public abstract class Extractor<T> where T : IMetricExtractor
    {
        protected readonly IList<T> Metrics;

        protected Extractor(IList<T> metrics)
        {
            Metrics = metrics ?? new List<T>();
        }

        public abstract string FileType();

        public abstract ProjectEntity Extract(string fileContent);
    }
}