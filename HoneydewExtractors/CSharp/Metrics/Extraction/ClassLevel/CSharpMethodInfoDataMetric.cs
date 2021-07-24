using System.Collections.Generic;
using HoneydewModels.CSharp;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpMethodInfoDataMetric
    {
        public IList<MethodModel> MethodInfos { get; } = new List<MethodModel>();
        public IList<MethodModel> ConstructorInfos { get; } = new List<MethodModel>();
    }
}
