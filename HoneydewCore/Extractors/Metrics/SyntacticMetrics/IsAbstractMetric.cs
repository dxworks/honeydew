using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics.SyntacticMetrics
{
    public class IsAbstractMetric : CSharpMetricExtractor, ISyntacticMetric
    {
        public bool IsAbstract { get; private set; }

        public override IMetric GetMetric()
        {
            return new Metric<bool>(IsAbstract);
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
                if (m.ValueText != "abstract") continue;
                any = true;
                break;
            }

            IsAbstract = IsAbstract || any;
        }
    }
}