using System.Linq;
using HoneydewCore.ModelRepresentations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class FieldsRelationVisitor : RelationMetricVisitor
    {
        public FieldsRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
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
                    CSharpHelperMethods.GetFullName(fieldDeclarationSyntax.Declaration.Type), this);
            }

            foreach (var eventFieldDeclarationSyntax in syntaxNode.DescendantNodes()
                .OfType<EventFieldDeclarationSyntax>())
            {
                MetricHolder.Add(className,
                    CSharpHelperMethods.GetFullName(eventFieldDeclarationSyntax.Declaration.Type), this);
            }
        }
    }
}
