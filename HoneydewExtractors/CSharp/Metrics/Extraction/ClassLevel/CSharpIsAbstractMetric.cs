using HoneydewExtractors.Core.Metrics;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.ClassLevel
{
    public class CSharpIsAbstractMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }
        public bool IsAbstract { get; private set; }

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.ClassLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<bool>(IsAbstract);
        }

        public override string PrettyPrint()
        {
            return "Is Abstract";
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            IsAbstract = true;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var any = false;
            foreach (var m in node.Modifiers)
            {
                if (m.ValueText != CSharpConstants.AbstractIdentifier) continue;
                any = true;
                break;
            }

            IsAbstract = IsAbstract || any;
        }
    }
}
