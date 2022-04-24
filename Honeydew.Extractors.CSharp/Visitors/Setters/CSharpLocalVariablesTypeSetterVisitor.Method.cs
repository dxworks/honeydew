using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public partial class CSharpLocalVariablesTypeSetterVisitor :
    ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax, IMethodType>,
    ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, DeclarationPatternSyntax, IMethodType>,
    ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, ForEachStatementSyntax, IMethodType>
{
    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        var missingLocalVariablesCount = 0;

        // variable declarator
        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
                IMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, VariableDeclaratorSyntax,
                    IMethodType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType, ref missingLocalVariablesCount);
        }

        // declaration pattern
        var declarationPatternSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
                IMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in declarationPatternSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, DeclarationPatternSyntax,
                    IMethodType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType, ref missingLocalVariablesCount);
        }

        // foreach statement
        var forEachStatementSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
                IMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in forEachStatementSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<MethodDeclarationSyntax, SemanticModel, ForEachStatementSyntax,
                    IMethodType>)this)
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
        ISetterVisitor<MethodDeclarationSyntax, SemanticModel, IMethodType, VariableDeclaratorSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return GetVariableDeclaratorSyntaxNodes(syntaxNode);
    }

    IEnumerable<DeclarationPatternSyntax>
        ISetterVisitor<MethodDeclarationSyntax, SemanticModel, IMethodType, DeclarationPatternSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return GetDeclarationPatternsSyntaxNodes(syntaxNode);
    }

    IEnumerable<ForEachStatementSyntax>
        ISetterVisitor<MethodDeclarationSyntax, SemanticModel, IMethodType, ForEachStatementSyntax, ILocalVariableType>.
        GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return GetForEachStatementsSyntaxNodes(syntaxNode);
    }
}
