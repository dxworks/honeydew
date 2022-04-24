using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public partial class CSharpLocalVariablesTypeSetterVisitor :
    ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
        IAccessorMethodType>,
    ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
        IAccessorMethodType>,
    ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
        IAccessorMethodType>
{
    public IAccessorMethodType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelType)
    {
        var missingLocalVariablesCount = 0;

        // variable declarator
        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
                IAccessorMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
                    IAccessorMethodType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariables(localVariableType);
        }

        // declaration pattern
        var declarationPatternSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
                IAccessorMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in declarationPatternSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
                    IAccessorMethodType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariables(localVariableType);
        }

        // foreach statement
        var forEachStatementSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
                IAccessorMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in forEachStatementSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
                    IAccessorMethodType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariables(localVariableType);
        }

        if (missingLocalVariablesCount > 0)
        {
            Logger.Log($"Could not set {missingLocalVariablesCount} local variables", LogLevels.Warning);
        }

        return modelType;

        void AddLocalVariables(ILocalVariableType? localVariableType)
        {
            if (localVariableType?.Type != null && !string.IsNullOrEmpty(localVariableType.Type.Name) &&
                localVariableType.Type.Name != CSharpConstants.VarIdentifier)
            {
                modelType.LocalVariableTypes.Add(localVariableType);
            }
            else
            {
                missingLocalVariablesCount++;
            }
        }
    }

    IEnumerable<VariableDeclaratorSyntax>
        ISetterVisitor<AccessorDeclarationSyntax, SemanticModel, IAccessorMethodType, VariableDeclaratorSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        return GetVariableDeclaratorSyntaxNodes(syntaxNode);
    }

    IEnumerable<DeclarationPatternSyntax>
        ISetterVisitor<AccessorDeclarationSyntax, SemanticModel, IAccessorMethodType, DeclarationPatternSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        return GetDeclarationPatternsSyntaxNodes(syntaxNode);
    }

    IEnumerable<ForEachStatementSyntax>
        ISetterVisitor<AccessorDeclarationSyntax, SemanticModel, IAccessorMethodType, ForEachStatementSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        return GetForEachStatementsSyntaxNodes(syntaxNode);
    }
}
