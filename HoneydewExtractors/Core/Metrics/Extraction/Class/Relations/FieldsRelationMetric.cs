using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class FieldsRelationMetric : RelationMetricVisitor
    {
        public FieldsRelationMetric(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Fields Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var fieldDeclarationSyntax in syntaxNode.DescendantNodes().OfType<FieldDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(fieldDeclarationSyntax.Declaration.Type));
            }

            foreach (var eventFieldDeclarationSyntax in syntaxNode.DescendantNodes()
                .OfType<EventFieldDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(eventFieldDeclarationSyntax.Declaration.Type));
            }
        }
    }
}
