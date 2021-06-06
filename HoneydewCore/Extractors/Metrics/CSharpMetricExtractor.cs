using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewCore.Extractors.Metrics
{
    public abstract class CSharpMetricExtractor : CSharpSyntaxWalker, IMetricExtractor
    {
        public abstract string GetName();
        public abstract int GetMetric();
    }
}