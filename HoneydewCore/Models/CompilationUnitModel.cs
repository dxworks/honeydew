using System.Collections.Generic;
using HoneydewCore.Extractors.Metrics;

namespace HoneydewCore.Models
{
    public class CompilationUnitModel
    {
        public IList<ClassModel> Entities { get; set; }
        public IDictionary<string, IMetric> Metrics { get; } = new Dictionary<string, IMetric>();
    }
}