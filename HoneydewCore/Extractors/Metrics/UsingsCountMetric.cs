using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewCore.Extractors.Metrics
{
    public class UsingsCountMetric : CSharpMetricExtractor
    {
        private int _usingsCount;

        public override string GetName()
        {
            return "Usings Count";
        }

        public override IMetric GetMetric()
        {
            return new Metric<int>(_usingsCount);
        }

        public override bool IsSemantic()
        {
            return false;
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            _usingsCount++;
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            base.VisitClassDeclaration(node);
        }
    }
}