using System.Collections.Generic;
using HoneydewModels.Types;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpMethodInfoDataMetric
    {
        public IList<IMethodType> MethodInfos { get; } = new List<IMethodType>();
        public IList<IConstructorType> ConstructorInfos { get; } = new List<IConstructorType>();
    }
}
