using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Models
{
    public record ClassModel
    {
        private string _namespace = "";

        public string FilePath { get; set; }
        public string FullName { get; set; }
        public IList<FieldModel> Fields { get; set; } = new List<FieldModel>();
        public IList<ClassMetric> Metrics { get; set; } = new List<ClassMetric>();

        public string Namespace
        {
            get
            {
                if (!string.IsNullOrEmpty(_namespace) || string.IsNullOrEmpty(FullName)) return _namespace;

                var lastIndexOf = FullName.LastIndexOf(".", StringComparison.Ordinal);
                if (lastIndexOf < 0)
                {
                    return "";
                }

                _namespace = FullName[..lastIndexOf];
                return _namespace;
            }
        }

        public Optional<object> GetMetricValue<T>() where T : IMetricExtractor
        {
            var firstOrDefault = Metrics.FirstOrDefault(metric => metric.ExtractorName == typeof(T).FullName);
            return firstOrDefault == default ? default(Optional<object>) : firstOrDefault.Value;
        }
    }
}