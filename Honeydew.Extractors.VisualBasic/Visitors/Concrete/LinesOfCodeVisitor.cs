using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class LinesOfCodeVisitor :
    IExtractionVisitor<PropertyStatementSyntax, SemanticModel, IPropertyType>,
    IExtractionVisitor<MethodStatementSyntax, SemanticModel, IMethodType>,
    IExtractionVisitor<AccessorBlockSyntax, SemanticModel, IAccessorMethodType>,
    IExtractionVisitor<ConstructorBlockSyntax, SemanticModel, IConstructorType>,
    IExtractionVisitor<MethodBlockSyntax, SemanticModel, IDestructorType>,
    IExtractionVisitor<ClassBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<StructureBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<InterfaceBlockSyntax, SemanticModel, IMembersClassType>,
    IExtractionVisitor<DelegateStatementSyntax, SemanticModel, IDelegateType>,
    IExtractionVisitor<EnumBlockSyntax, SemanticModel, IEnumType>,
    IExtractionVisitor<CompilationUnitSyntax, SemanticModel, ICompilationUnitType>
{
    private readonly VisualBasicLinesOfCodeCounter _linesOfCodeCounter = new();

    public IPropertyType Visit(PropertyStatementSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        var propertyBlockSyntax = syntaxNode.GetParentDeclarationSyntax<PropertyBlockSyntax>();
        modelType.Loc = _linesOfCodeCounter.Count(propertyBlockSyntax is not null
            ? propertyBlockSyntax.ToString()
            : syntaxNode.ToString());

        return modelType;
    }

    public IMethodType Visit(MethodStatementSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        var methodBlockSyntax = syntaxNode.GetParentDeclarationSyntax<MethodBlockSyntax>();
        modelType.Loc =
            _linesOfCodeCounter.Count(methodBlockSyntax is not null
                ? methodBlockSyntax.ToString()
                : syntaxNode.ToString());
        return modelType;
    }

    public IAccessorMethodType Visit(AccessorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelMethodType)
    {
        modelMethodType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelMethodType;
    }

    public IConstructorType Visit(ConstructorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IDestructorType Visit(MethodBlockSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IMembersClassType Visit(ClassBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IMembersClassType Visit(StructureBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IMembersClassType Visit(InterfaceBlockSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IDelegateType Visit(DelegateStatementSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public IEnumType Visit(EnumBlockSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
    {
        modelType.Loc = _linesOfCodeCounter.Count(syntaxNode.ToString());
        return modelType;
    }

    public ICompilationUnitType Visit(CompilationUnitSyntax syntaxNode, SemanticModel semanticModel,
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
