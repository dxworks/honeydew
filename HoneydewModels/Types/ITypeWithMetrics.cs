using System.Collections.Generic;
using HoneydewModels.CSharp;

namespace HoneydewModels.Types
{
    public interface ITypeWithMetrics : IType
    {
        public IList<MetricModel> Metrics { get; init; }
    }
}
