using System;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common;

public class GotoStatementVisitor : ICSharpMethodVisitor, ICSharpConstructorVisitor, ICSharpDestructorVisitor,
    ICSharpMethodAccessorVisitor, ICSharpLocalFunctionVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, IDestructorType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IMethodType Visit(AccessorDeclarationSyntax syntaxNode, IMethodType modelType)
    {
        modelType.Metrics.Add(CalculateGotoStatements(syntaxNode));

        return modelType;
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode,
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
        {
            ExtractorName = nameof(GotoStatementVisitor),
            ValueType = nameof(Int32),
            Value = gotoStatementsCount
        });
        
        return modelType;
    }

    private static MetricModel CalculateGotoStatements(SyntaxNode syntaxNode)
    {
        var gotoStatementsCount = syntaxNode.DescendantNodes()
            .Where(descendantNode => descendantNode.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>() == null)
            .OfType<GotoStatementSyntax>().Count();

        return new MetricModel
        {
            ExtractorName = nameof(GotoStatementVisitor),
            ValueType = nameof(Int32),
            Value = gotoStatementsCount
        };
    }
}
