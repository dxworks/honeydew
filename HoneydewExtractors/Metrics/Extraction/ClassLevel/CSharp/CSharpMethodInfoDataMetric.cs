using System.Collections.Generic;
using HoneydewModels;

namespace HoneydewExtractors.Metrics.Extraction.ClassLevel.CSharp
{
    public class CSharpMethodInfoDataMetric
    {
        public IList<MethodModel> MethodInfos { get; } = new List<MethodModel>();
        public IList<MethodModel> ConstructorInfos { get; } = new List<MethodModel>();
    }
}
