using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel
{
    public class CSharpUsingsCountMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }

        public int UsingsCount { get; private set; }

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.CompilationUnitLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<int>(UsingsCount);
        }

        public override string PrettyPrint()
        {
            return "Usings Count";
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            UsingsCount++;
        }
    }
}
