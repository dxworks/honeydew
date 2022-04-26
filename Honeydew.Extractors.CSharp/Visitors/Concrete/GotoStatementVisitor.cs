using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class GotoStatementVisitor : IExtractionVisitor<MethodDeclarationSyntax, SemanticModel, IMethodType>,
    IExtractionVisitor<ConstructorDeclarationSyntax, SemanticModel, IConstructorType>,
    IExtractionVisitor<DestructorDeclarationSyntax, SemanticModel, IDestructorType>,
    IExtractionVisitor<AccessorDeclarationSyntax, SemanticModel, IAccessorMethodType>,
    IExtractionVisitor<LocalFunctionStatementSyntax, SemanticModel, IMethodTypeWithLocalFunctions>
{
    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IAccessorMethodType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelMethodType)
    {
        modelMethodType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelMethodType;
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        if (syntaxNode.Body == null)
        {
            return modelType;
        }

        var gotoStatementsCount = syntaxNode.Body.DescendantNodes()
            .Where(node => node.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == syntaxNode)
            .OfType<GotoStatementSyntax>().Count();

        modelType.Metrics.Add(new MetricModel
        (
            "GotoStatementsCount",
            typeof(GotoStatementVisitor).FullName ?? nameof(GotoStatementVisitor),
            typeof(int).FullName ?? nameof(Int32),
            gotoStatementsCount
        ));

        return modelType;
    }

    private static MetricModel CalculateGotoStatements(SyntaxNode syntaxNode)
    {
        var gotoStatementsCount = syntaxNode.DescendantNodes()
            .Where(descendantNode => descendantNode.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == null)
            .OfType<GotoStatementSyntax>().Count();

        return new MetricModel
        (
            "GotoStatementsCount",
            typeof(GotoStatementVisitor).FullName ?? nameof(GotoStatementVisitor),
            typeof(int).FullName ?? nameof(Int32),
            gotoStatementsCount
        );
    }
}
