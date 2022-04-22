using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class LinesOfCodeVisitor : ICSharpPropertyVisitor, ICSharpMethodVisitor, ICSharpConstructorVisitor,
    ICSharpClassVisitor, ICSharpCompilationUnitVisitor, ICSharpLocalFunctionVisitor, ICSharpMethodAccessorVisitor,
    ICSharpDestructorVisitor, ICSharpDelegateVisitor, ICSharpEnumVisitor
{
    private readonly CSharpLinesOfCodeCounter _linesOfCodeCounter = new();

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

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
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

    public IEnumType Visit(EnumDeclarationSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
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
}
