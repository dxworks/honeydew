using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class ObjectCreationRelationVisitor : IExtractionVisitor<TypeDeclarationSyntax, SemanticModel, IMembersClassType>
{
    public const string ObjectCreationDependencyMetricName = "ObjectCreationDependency";

    private readonly ILogger _logger;

    public ObjectCreationRelationVisitor(ILogger logger)
    {
        _logger = logger;
    }

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

    private IDictionary<string, int> AddDependencies(BaseTypeDeclarationSyntax syntaxNode,
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
            if (expressionSyntax is null)
            {
                _logger.Log($"Initializer Expression Syntax is null for {syntaxNode.Identifier.ToString()}");
                continue;
            }

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
