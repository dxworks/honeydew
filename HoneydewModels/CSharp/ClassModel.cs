using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;

namespace HoneydewModels.CSharp
{
    public record ClassModel : IPropertyMembersClassType, IModelEntity
    {
        public string ClassType { get; set; }

        public string Name { get; set; }

        public string FilePath { get; set; }

        public LinesOfCode Loc { get; set; }

        public string AccessModifier { get; set; }

        public string Modifier { get; set; } = "";

        public string ContainingTypeName
        {
            get => Namespace;
            set => _namespace = value;
        }

        public IList<IBaseType> BaseTypes { get; set; } = new List<IBaseType>();

        public IList<IImportType> Imports { get; set; } = new List<IImportType>();

        public IList<IFieldType> Fields { get; init; } = new List<IFieldType>();

        public IList<IPropertyType> Properties { get; set; } = new List<IPropertyType>();

        public IList<IConstructorType> Constructors { get; init; } = new List<IConstructorType>();

        public IList<IMethodType> Methods { get; init; } = new List<IMethodType>();

        public IList<MetricModel> Metrics { get; init; } = new List<MetricModel>();

        public IList<IAttributeType> Attributes { get; set; } = new List<IAttributeType>();

        public string Namespace
        {
            get
            {
                if (!string.IsNullOrEmpty(_namespace) || string.IsNullOrEmpty(Name)) return _namespace;

                var lastIndexOf = Name.LastIndexOf(".", StringComparison.Ordinal);
                if (lastIndexOf < 0)
                {
                    return "";
                }

                _namespace = Name[..lastIndexOf];
                return _namespace;
            }
        }

        private string _namespace = "";

        public void AddMetricValue(string extractorName, IMetricValue metricValue)
        {
            Metrics.Add(new MetricModel
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
