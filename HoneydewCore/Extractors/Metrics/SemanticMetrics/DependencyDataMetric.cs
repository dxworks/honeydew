using System.Collections.Generic;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class DependencyDataMetric
    {
        public IList<string> Usings = new List<string>();
        public IDictionary<string, int> Dependencies { get; set; } = new Dictionary<string, int>();
    }
}