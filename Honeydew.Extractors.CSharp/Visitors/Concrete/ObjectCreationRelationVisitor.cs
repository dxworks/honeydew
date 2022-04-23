using Honeydew.Extractors.CSharp.Utils;
using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class ObjectCreationRelationVisitor : IExtractionVisitor<TypeDeclarationSyntax, SemanticModel, IMembersClassType>
{
    public const string ObjectCreationDependencyMetricName = "ObjectCreationDependency";

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var dictionary = AddDependencies(syntaxNode, semanticModel);

        modelType.Metrics.Add(new MetricModel(
            ObjectCreationDependencyMetricName,
            GetType().ToString(),
            dictionary.GetType().ToString(),
            dictionary
        ));

        return modelType;
    }

    private static IDictionary<string, int> AddDependencies(BaseTypeDeclarationSyntax syntaxNode,
        SemanticModel semanticModel)
    {
        var dictionary = new Dictionary<string, int>();

        foreach (var objectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ObjectCreationExpressionSyntax>())
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetFullName(objectCreationExpressionSyntax.Type, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                AddDependency(dictionary, dependencyName);
            }
        }

        foreach (var implicitObjectCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ImplicitObjectCreationExpressionSyntax>())
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetFullName(implicitObjectCreationExpressionSyntax, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                AddDependency(dictionary, dependencyName);
            }
        }

        foreach (var arrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ArrayCreationExpressionSyntax>())
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetFullName(arrayCreationExpressionSyntax.Type, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                AddDependency(dictionary, dependencyName);
            }
        }

        foreach (var implicitArrayCreationExpressionSyntax in syntaxNode.DescendantNodes()
                     .OfType<ImplicitArrayCreationExpressionSyntax>())
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetFullName(implicitArrayCreationExpressionSyntax, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                AddDependency(dictionary, dependencyName);
            }
        }

        foreach (var expressionSyntax in syntaxNode.DescendantNodes().OfType<InitializerExpressionSyntax>()
                     .Where(syntax => syntax.Parent is EqualsValueClauseSyntax)
                     .Select(syntax => syntax.Parent))
        {
            var dependencyName = CSharpExtractionHelperMethods
                .GetContainingType(expressionSyntax, semanticModel).Name;
            if (dependencyName != CSharpConstants.VarIdentifier)
            {
                AddDependency(dictionary, dependencyName);
            }
        }

        return dictionary;
    }

    private static void AddDependency(IDictionary<string, int> dependencies, string dependencyName)
    {
        dependencyName = dependencyName.Trim('?');
        if (dependencies.ContainsKey(dependencyName))
        {
            dependencies[dependencyName]++;
        }
        else
        {
            dependencies.Add(dependencyName, 1);
        }
    }
}
