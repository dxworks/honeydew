using System.Collections.Generic;

namespace HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp
{
    public class CSharpInheritanceMetric
    {
        public IList<string> Interfaces { get; set; } = new List<string>();
        public string BaseClassName { get; set; }
    }
}
