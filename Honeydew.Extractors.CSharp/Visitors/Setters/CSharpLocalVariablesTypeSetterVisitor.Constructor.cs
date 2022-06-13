using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public partial class CSharpLocalVariablesTypeSetterVisitor :
    ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
        IConstructorType>,
    ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
        IConstructorType>,
    ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
        IConstructorType>
{
    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        var missingLocalVariablesCount = 0;

        // variable declarator
        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
                IConstructorType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax
                    , IConstructorType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariables(localVariableType);
        }

        // declaration pattern
        var declarationPatternSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
                IConstructorType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in declarationPatternSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, DeclarationPatternSyntax
                    , IConstructorType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariables(localVariableType);
        }

        // foreach statement
        var forEachStatementSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
                IConstructorType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in forEachStatementSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
                    IConstructorType>)this)
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
        ISetterVisitor<ConstructorDeclarationSyntax, SemanticModel, IConstructorType, VariableDeclaratorSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        return GetVariableDeclaratorSyntaxNodes(syntaxNode);
    }

    IEnumerable<DeclarationPatternSyntax>
        ISetterVisitor<ConstructorDeclarationSyntax, SemanticModel, IConstructorType, DeclarationPatternSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        return GetDeclarationPatternsSyntaxNodes(syntaxNode);
    }

    IEnumerable<ForEachStatementSyntax>
        ISetterVisitor<ConstructorDeclarationSyntax, SemanticModel, IConstructorType, ForEachStatementSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        return GetForEachStatementsSyntaxNodes(syntaxNode);
    }
}
