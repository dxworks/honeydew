using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Extractors.Metrics;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis;

namespace HoneydewCore.Models
{
    public record ClassModel
    {
        public string ClassType { get; init; }

        public string FilePath { get; set; }

        public string FullName { get; set; }

        public string AccessModifier { get; init; }

        public string Modifier { get; init; } = "";

        public string BaseClassFullName { get; set; } = CSharpConstants.ObjectIdentifier;

        public IList<string> BaseInterfaces { get; init; } = new List<string>();

        public IList<FieldModel> Fields { get; init; } = new List<FieldModel>();

        public IList<MethodModel> Constructors { get; init; } = new List<MethodModel>();
        
        public IList<MethodModel> Methods { get; init; } = new List<MethodModel>();

        public IList<ClassMetric> Metrics { get; init; } = new List<ClassMetric>();


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

        private string _namespace = "";

        public Optional<object> GetMetricValue<T>() where T : IMetricExtractor
        {
            var firstOrDefault = Metrics.FirstOrDefault(metric => metric.ExtractorName == typeof(T).FullName);
            return firstOrDefault == default ? default(Optional<object>) : firstOrDefault.Value;
        }
    }
}