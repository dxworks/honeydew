using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HoneydewCore.Extractors.Metrics
{
    public abstract class CSharpMetricExtractor : CSharpSyntaxWalker, IMetricExtractor
    {
        public SemanticModel SemanticModel { get; set; }

        public abstract IMetric GetMetric();
        public abstract string PrettyPrint();
    }
}