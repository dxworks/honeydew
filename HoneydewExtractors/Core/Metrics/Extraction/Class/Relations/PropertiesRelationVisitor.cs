using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class PropertiesRelationVisitor : RelationMetricVisitor
    {
        public PropertiesRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Properties Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var propertyDeclarationSyntax in syntaxNode.DescendantNodes().OfType<PropertyDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(propertyDeclarationSyntax.Type));
            }

            foreach (var eventDeclarationSyntax in syntaxNode.DescendantNodes()
                .OfType<EventDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(eventDeclarationSyntax.Type));
            }        }
    }
}
