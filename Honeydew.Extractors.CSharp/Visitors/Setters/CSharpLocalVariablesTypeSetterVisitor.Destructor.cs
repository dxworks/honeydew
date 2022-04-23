using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public partial class CSharpLocalVariablesTypeSetterVisitor :
    ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
        IDestructorType>,
    ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
        IDestructorType>,
    ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
        IDestructorType>
{
    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        var missingLocalVariablesCount = 0;

        // variable declarator
        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
                IDestructorType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
                    IDestructorType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType, ref missingLocalVariablesCount);
        }

        // declaration pattern
        var declarationPatternSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
                IDestructorType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in declarationPatternSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
                    IDestructorType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType, ref missingLocalVariablesCount);
        }

        // foreach statement
        var forEachStatementSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
                IDestructorType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in forEachStatementSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
                    IDestructorType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType, ref missingLocalVariablesCount);
        }

        if (missingLocalVariablesCount > 0)
        {
            Logger.Log($"Could not set {missingLocalVariablesCount} local variables", LogLevels.Warning);
        }

        return modelType;
    }

    IEnumerable<VariableDeclaratorSyntax>
        ISetterVisitor<DestructorDeclarationSyntax, SemanticModel, IDestructorType, VariableDeclaratorSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(DestructorDeclarationSyntax syntaxNode)
    {
        return GetVariableDeclaratorSyntaxNodes(syntaxNode);
    }

    IEnumerable<DeclarationPatternSyntax>
        ISetterVisitor<DestructorDeclarationSyntax, SemanticModel, IDestructorType, DeclarationPatternSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(DestructorDeclarationSyntax syntaxNode)
    {
        return GetDeclarationPatternsSyntaxNodes(syntaxNode);
    }

    IEnumerable<ForEachStatementSyntax>
        ISetterVisitor<DestructorDeclarationSyntax, SemanticModel, IDestructorType, ForEachStatementSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(DestructorDeclarationSyntax syntaxNode)
    {
        return GetForEachStatementsSyntaxNodes(syntaxNode);
    }
}
