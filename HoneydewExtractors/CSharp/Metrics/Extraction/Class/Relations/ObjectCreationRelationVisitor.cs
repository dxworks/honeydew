using System.Linq;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations
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
                var dependencyName = CSharpHelperMethods.GetFullName(objectCreationExpressionSyntax.Type).Name;
                if (dependencyName != CSharpConstants.VarIdentifier)
                {
                    MetricHolder.Add(className, dependencyName, this);
                }
            }

            foreach (var implicitObjectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ImplicitObjectCreationExpressionSyntax>())
            {
                var dependencyName = CSharpHelperMethods.GetFullName(implicitObjectCreationExpressionSyntax).Name;
                if (dependencyName != CSharpConstants.VarIdentifier)
                {
                    MetricHolder.Add(className, dependencyName, this);
                }
            }

            foreach (var arrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ArrayCreationExpressionSyntax>())
            {
                var dependencyName = CSharpHelperMethods.GetFullName(arrayCreationExpressionSyntax.Type).Name;
                if (dependencyName != CSharpConstants.VarIdentifier)
                {
                    MetricHolder.Add(className,
                        dependencyName, this);
                }
            }

            foreach (var implicitArrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                .OfType<ImplicitArrayCreationExpressionSyntax>())
            {
                var dependencyName = CSharpHelperMethods.GetFullName(implicitArrayCreationExpressionSyntax).Name;
                if (dependencyName != CSharpConstants.VarIdentifier)
                {
                    MetricHolder.Add(className, dependencyName, this);
                }
            }

            foreach (var expressionSyntax in syntaxNode.DescendantNodes().OfType<InitializerExpressionSyntax>()
                .Where(syntax => syntax.Parent is EqualsValueClauseSyntax)
                .Select(syntax => syntax.Parent))
            {
                var dependencyName = CSharpHelperMethods.GetContainingType(expressionSyntax).Name;
                if (dependencyName != CSharpConstants.VarIdentifier)
                {
                    MetricHolder.Add(className, dependencyName, this);
                }
            }
        }
    }
}
