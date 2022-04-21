using System.Collections.Generic;

namespace Honeydew.Models.Types;

public interface ITypeWithMetrics : IType
{
    public IList<MetricModel> Metrics { get; init; }
}
