using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public class CompilationUnitModel
    {
        public IList<ClassModel> ClassModels { get; } = new List<ClassModel>();
        public MetricsSet SyntacticMetrics { get; } = new();
    }
}