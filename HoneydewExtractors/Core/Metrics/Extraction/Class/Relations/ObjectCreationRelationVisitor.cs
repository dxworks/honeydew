using System.Linq;
using HoneydewCore.ModelRepresentations;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class ObjectCreationRelationVisitor : RelationVisitor
    {
        public ObjectCreationRelationVisitor()
        {
        }

        public ObjectCreationRelationVisitor(IRelationMetricHolder metricHolder) : base(metricHolder)
        {
        }

        public override string PrettyPrint()
        {
            return "Object Creation Dependency";
        }

        protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode)
        {
            foreach (var objectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>())
            {
                MetricHolder.Add(className,
                    CSharpHelperMethods.GetFullName(objectCreationExpressionSyntax.Type).Name, this);
            }

            foreach (var implicitObjectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ImplicitObjectCreationExpressionSyntax>())
            {
                MetricHolder.Add(className,
                    CSharpHelperMethods.GetFullName(implicitObjectCreationExpressionSyntax).Name, this);
            }

            foreach (var arrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ArrayCreationExpressionSyntax>())
            {
                MetricHolder.Add(className,
                    CSharpHelperMethods.GetFullName(arrayCreationExpressionSyntax.Type).Name, this);
            }

            foreach (var implicitArrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ImplicitArrayCreationExpressionSyntax>())
            {
                MetricHolder.Add(className,
                    CSharpHelperMethods.GetFullName(implicitArrayCreationExpressionSyntax).Name, this);
            }

            foreach (var expressionSyntax in syntaxNode.DescendantNodes().OfType<InitializerExpressionSyntax>()
                .Where(syntax => syntax.Parent is EqualsValueClauseSyntax)
                .Select(syntax => syntax.Parent))
            {
                MetricHolder.Add(className, CSharpHelperMethods.GetContainingType(expressionSyntax).Name, this);
            }
        }
    }
}
