using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;

namespace HoneydewCore.Models
{
    public class ClassModel : ProjectEntity
    {
        public string Namespace { get; init; }

        public Dictionary<string, IMetric> Metrics { get; } = new();
    }
}