using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace HoneydewModels.CSharp
{
    public record ClassModel : IClassModel
    {
        public string ClassType { get; init; }

        public string FilePath { get; set; }

        public string FullName { get; set; }

        public string AccessModifier { get; init; }

        public string Modifier { get; init; } = "";

        public string BaseClassFullName { get; set; } = "object";

        public IList<string> BaseInterfaces { get; init; } = new List<string>();

        public IList<FieldModel> Fields { get; init; } = new List<FieldModel>();
        
        public IList<PropertyModel> Properties { get; init; } = new List<PropertyModel>();

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

        public void AddMetricValue(string extractorName, IMetricValue metricValue)
        {
            Metrics.Add(new ClassMetric
            {
                ExtractorName = extractorName,
                ValueType = metricValue.GetValueType(),
                Value = metricValue.GetValue()
            });
        }

        public Optional<object> GetMetricValue<T>()
        {
            var firstOrDefault = Metrics.FirstOrDefault(metric => metric.ExtractorName == typeof(T).FullName);
            return firstOrDefault == default ? default(Optional<object>) : firstOrDefault.Value;
        }
    }
}
