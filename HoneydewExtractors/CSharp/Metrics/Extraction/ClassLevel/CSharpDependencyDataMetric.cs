using System.Collections.Generic;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpDependencyDataMetric
    {
        public IList<string> Usings { get; set; } = new List<string>();
        public IDictionary<string, int> Dependencies { get; set; } = new Dictionary<string, int>();
    }
}
