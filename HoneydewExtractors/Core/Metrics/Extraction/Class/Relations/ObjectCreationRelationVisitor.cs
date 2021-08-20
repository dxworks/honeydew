using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Class.Relations
{
    public class ObjectCreationRelationVisitor : RelationMetricVisitor
    {
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
                    InheritedSemanticModel.GetFullName(objectCreationExpressionSyntax.Type));
            }

            foreach (var implicitObjectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ImplicitObjectCreationExpressionSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(implicitObjectCreationExpressionSyntax));
            }

            foreach (var arrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ArrayCreationExpressionSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(arrayCreationExpressionSyntax.Type));
            }

            foreach (var implicitArrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ImplicitArrayCreationExpressionSyntax>())
            {
                MetricHolder.Add(className,
                    InheritedSemanticModel.GetFullName(implicitArrayCreationExpressionSyntax));
            }

            foreach (var expressionSyntax in syntaxNode.DescendantNodes().OfType<InitializerExpressionSyntax>()
                .Where(syntax => syntax.Parent is EqualsValueClauseSyntax)
                .Select(syntax => syntax.Parent))
            {
                MetricHolder.Add(className, InheritedSemanticModel.GetContainingType(expressionSyntax));
            }
        }
    }
}
