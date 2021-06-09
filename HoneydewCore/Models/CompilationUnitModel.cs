using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public class CompilationUnitModel
    {
        public IList<ClassModel> ClassModels { get; set; }
        public MetricsSet SyntacticMetrics { get; } = new();
    }
}