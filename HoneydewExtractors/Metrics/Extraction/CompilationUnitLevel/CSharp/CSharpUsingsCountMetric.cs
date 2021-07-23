using HoneydewExtractors.Metrics.CSharp;
using HoneydewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Metrics.Extraction.CompilationUnitLevel.CSharp
{
    public class CSharpUsingsCountMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>, IUsingsCountMetric
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
