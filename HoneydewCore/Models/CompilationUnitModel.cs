using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;

namespace HoneydewCore.Models
{
    public class CompilationUnitModel
    {
        public IList<ClassModel> Entities { get; set; }
        public IDictionary<string, IMetric> SyntacticMetrics { get; } = new Dictionary<string, IMetric>();
    }
}