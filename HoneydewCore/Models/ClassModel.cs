using System.Collections.Generic;

namespace HoneydewCore.Models
{
    public class ClassModel : ProjectEntity
    {
        public string Namespace { get; init; }

        public Dictionary<string, int> Metrics { get; } = new();
    }
}