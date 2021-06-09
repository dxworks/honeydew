using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public record FinalClassModel
    {
        public string FullName { get; set; }
        public IList<ClassMetric> Metrics { get; set; } = new List<ClassMetric>();
    }
}