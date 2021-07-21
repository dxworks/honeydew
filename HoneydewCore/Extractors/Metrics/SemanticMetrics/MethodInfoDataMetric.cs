using System.Collections.Generic;
using HoneydewCore.Models;
using HoneydewModels;

namespace HoneydewCore.Extractors.Metrics.SemanticMetrics
{
    public class MethodInfoDataMetric
    {
        public IList<MethodModel> MethodInfos { get; } = new List<MethodModel>();
        public IList<MethodModel> ConstructorInfos { get; } = new List<MethodModel>();
    }
}
