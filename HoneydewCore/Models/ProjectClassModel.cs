using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public record ProjectClassModel
    {
        public string Path { get; set; }
        public string FullName { get; set; }
        public IList<ClassMetric> Metrics { get; set; } = new List<ClassMetric>();
    }
}