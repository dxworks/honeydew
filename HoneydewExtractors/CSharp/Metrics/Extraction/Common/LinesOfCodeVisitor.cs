using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.CompilationUnit;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics.Visitors.Method;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common;

public class LinesOfCodeVisitor : ICSharpPropertyVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor,
    ICSharpClassVisitor, ICSharpCompilationUnitVisitor, ICSharpLocalFunctionVisitor, ICSharpMethodAccessorVisitor,
    ICSharpDestructorVisitor, ICSharpDelegateVisitor
{
    private readonly CSharpLinesOfCodeCounter _linesOfCodeCounter = new();
    private bool _visited;

    public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IAccessorType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public ICompilationUnitType Visit(CSharpSyntaxNode syntaxNode, SemanticModel semanticModel,
        ICompilationUnitType modelType)
    {
        var linesOfCode = _linesOfCodeCounter.Count(syntaxNode.ToString());

        if (syntaxNode.HasLeadingTrivia)
        {
            var loc = _linesOfCodeCounter.Count(syntaxNode.GetLeadingTrivia().ToString());
            linesOfCode.SourceLines += loc.SourceLines;
            linesOfCode.CommentedLines += loc.CommentedLines;
            linesOfCode.EmptyLines += loc.EmptyLines;
        }

        modelType.Loc = linesOfCode;
        return modelType;
    }

    public void Accept(IVisitor visitor)
    {
        if (_visited)
        {
            return;
        }

        _visited = true;
        visitor.Visit(this);
    }
}
