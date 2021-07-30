using System.Collections.Generic;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpInheritanceMetric
    {
        public IList<string> Interfaces { get; set; } = new List<string>();
        public string BaseClassName { get; set; }
    }
}
