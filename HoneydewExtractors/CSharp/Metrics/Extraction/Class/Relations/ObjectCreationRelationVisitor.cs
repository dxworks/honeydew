using System.Linq;
using HoneydewCore.ModelRepresentations;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

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

    protected override void AddDependencies(string className, BaseTypeDeclarationSyntax syntaxNode,
        SemanticModel semanticModel)
    {
        foreach (var objectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ObjectCreationExpressionSyntax>())
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetFullName(objectCreationExpressionSyntax.Type, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                MetricHolder.Add(className, dependencyName, this);
            }
        }

        foreach (var implicitObjectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ImplicitObjectCreationExpressionSyntax>())
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetFullName(implicitObjectCreationExpressionSyntax, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                MetricHolder.Add(className, dependencyName, this);
            }
        }

        foreach (var arrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ArrayCreationExpressionSyntax>())
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetFullName(arrayCreationExpressionSyntax.Type, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                MetricHolder.Add(className,
                    dependencyName, this);
            }
        }

        foreach (var implicitArrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ImplicitArrayCreationExpressionSyntax>())
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetFullName(implicitArrayCreationExpressionSyntax, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                MetricHolder.Add(className, dependencyName, this);
            }
        }

        foreach (var expressionSyntax in syntaxNode.DescendantNodes().OfType<InitializerExpressionSyntax>()
                     .Where(syntax => syntax.Parent is EqualsValueClauseSyntax)
                     .Select(syntax => syntax.Parent))
        {
            var dependencyName = CSharpExtractionHelperMethods.GetContainingType(expressionSyntax, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                MetricHolder.Add(className, dependencyName, this);
            }
        }
    }
}
