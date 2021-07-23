using System.Collections.Generic;

namespace HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp
{
    public class CSharpDependencyDataMetric
    {
        public IList<string> Usings { get; set; } = new List<string>();
        public IDictionary<string, int> Dependencies { get; set; } = new Dictionary<string, int>();
    }
}
