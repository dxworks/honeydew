using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class
    ExceptionsThrownRelationVisitor : IExtractionVisitor<TypeDeclarationSyntax, SemanticModel, IMembersClassType>
{
    public const string ExceptionsThrownDependencyMetricName = "ExceptionsThrownDependency";

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        var dictionary = AddDependencies(syntaxNode, semanticModel);

        modelType.Metrics.Add(new MetricModel(
            ExceptionsThrownDependencyMetricName,
            GetType().ToString(),
            dictionary.GetType().ToString(),
            dictionary
        ));

        return modelType;
    }

    private IDictionary<string, int> AddDependencies(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel)
    {
        var dictionary = new Dictionary<string, int>();

        foreach (var throwExpressionSyntax in syntaxNode.DescendantNodes().OfType<ThrowExpressionSyntax>())
        {
            AddDependency(dictionary,
                CSharpExtractionHelperMethods
                    .GetFullName(throwExpressionSyntax.Expression, semanticModel).Name);
        }

        foreach (var throwStatementSyntax in syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>())
        {
            AddDependency(dictionary,
                throwStatementSyntax.Expression == null
                    ? CSharpExtractionHelperMethods
                        .GetFullName(throwStatementSyntax, semanticModel).Name
                    : CSharpExtractionHelperMethods
                        .GetFullName(throwStatementSyntax.Expression, semanticModel).Name);
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
