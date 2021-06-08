using System.Collections.Generic;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class InheritanceMetric
    {
        public IList<string> Interfaces { get; } = new List<string>();
        public string BaseClassName { get; set; }
    }
}