using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class GotoStatementVisitor :
    IExtractionVisitor<MethodStatementSyntax, SemanticModel, IMethodType>,
    IExtractionVisitor<ConstructorBlockSyntax, SemanticModel, IConstructorType>,
    IExtractionVisitor<MethodBlockSyntax, SemanticModel, IDestructorType>,
    IExtractionVisitor<AccessorBlockSyntax, SemanticModel, IAccessorMethodType>
{
    public IMethodType Visit(MethodStatementSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        if (syntaxNode.Parent is MethodBlockSyntax methodBlockSyntax)
        {
            modelType.Metrics.Add(CalculateGotoStatements(methodBlockSyntax));
        }

        return modelType;
    }

    public IConstructorType Visit(ConstructorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IDestructorType Visit(MethodBlockSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IAccessorMethodType Visit(AccessorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelMethodType)
    {
        modelMethodType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelMethodType;
    }

    private static MetricModel CalculateGotoStatements(SyntaxNode syntaxNode)
    {
        var gotoStatementsCount = syntaxNode.DescendantNodes()
            .OfType<GoToStatementSyntax>().Count();

        return new MetricModel
        (
            "GotoStatementsCount",
            typeof(GotoStatementVisitor).FullName ?? nameof(GotoStatementVisitor),
            typeof(int).FullName ?? nameof(Int32),
            gotoStatementsCount
        );
    }
}
