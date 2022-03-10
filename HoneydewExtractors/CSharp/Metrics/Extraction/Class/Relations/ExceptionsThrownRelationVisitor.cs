using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Class.Relations;

public class ExceptionsThrownRelationVisitor : ICSharpClassVisitor
{
    public const string ExceptionsThrownDependencyMetricName = "ExceptionsThrownDependency";

    public void Accept(IVisitor visitor)
    {
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
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
                CSharpExtractionHelperMethods.GetFullName(throwExpressionSyntax.Expression, semanticModel).Name);
        }

        foreach (var throwStatementSyntax in syntaxNode.DescendantNodes().OfType<ThrowStatementSyntax>())
        {
            AddDependency(dictionary,
                throwStatementSyntax.Expression == null
                    ? CSharpExtractionHelperMethods.GetFullName(throwStatementSyntax, semanticModel).Name
                    : CSharpExtractionHelperMethods.GetFullName(throwStatementSyntax.Expression, semanticModel).Name);
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
