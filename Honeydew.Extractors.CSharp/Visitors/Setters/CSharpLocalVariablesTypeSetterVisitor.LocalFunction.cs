using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public partial class CSharpLocalVariablesTypeSetterVisitor :
    ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, VariableDeclaratorSyntax,
        IMethodTypeWithLocalFunctions>,
    ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, DeclarationPatternSyntax,
        IMethodTypeWithLocalFunctions>,
    ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, ForEachStatementSyntax,
        IMethodTypeWithLocalFunctions>
{
    public IMethodTypeWithLocalFunctions Visit(LocalFunctionStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodTypeWithLocalFunctions modelType)
    {
        var missingLocalVariablesCount = 0;

        // variable declarator
        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, VariableDeclaratorSyntax,
                IMethodTypeWithLocalFunctions>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, VariableDeclaratorSyntax
                    , IMethodTypeWithLocalFunctions>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType, ref missingLocalVariablesCount);
        }

        // declaration pattern
        var declarationPatternSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, DeclarationPatternSyntax,
                IMethodTypeWithLocalFunctions>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in declarationPatternSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, DeclarationPatternSyntax
                    , IMethodTypeWithLocalFunctions>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType, ref missingLocalVariablesCount);
        }

        // foreach statement
        var forEachStatementSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, ForEachStatementSyntax,
                IMethodTypeWithLocalFunctions>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in forEachStatementSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, ForEachStatementSyntax,
                    IMethodTypeWithLocalFunctions>)this)
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
        ISetterVisitor<LocalFunctionStatementSyntax, SemanticModel, IMethodTypeWithLocalFunctions,
            VariableDeclaratorSyntax, ILocalVariableType>.GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        if (syntaxNode.Body == null)
        {
            return Enumerable.Empty<VariableDeclaratorSyntax>();
        }

        return syntaxNode.Body
            .ChildNodes()
            .OfType<LocalDeclarationStatementSyntax>()
            .SelectMany(syntax => syntax.DescendantNodes().OfType<VariableDeclaratorSyntax>());
    }

    IEnumerable<DeclarationPatternSyntax>
        ISetterVisitor<LocalFunctionStatementSyntax, SemanticModel, IMethodTypeWithLocalFunctions,
            DeclarationPatternSyntax, ILocalVariableType>.GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        if (syntaxNode.Body == null)
        {
            yield break;
        }

        foreach (var declarationPatternSyntax in syntaxNode.Body
                     .DescendantNodes()
                     .OfType<DeclarationPatternSyntax>())
        {
            var parentDeclarationSyntax =
                declarationPatternSyntax.GetParentDeclarationSyntax<LocalFunctionStatementSyntax>();
            if (parentDeclarationSyntax != syntaxNode)
            {
                continue;
            }

            yield return declarationPatternSyntax;
        }
    }

    IEnumerable<ForEachStatementSyntax>
        ISetterVisitor<LocalFunctionStatementSyntax, SemanticModel, IMethodTypeWithLocalFunctions,
            ForEachStatementSyntax, ILocalVariableType>.GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    {
        return syntaxNode.Body == null
            ? Enumerable.Empty<ForEachStatementSyntax>()
            : syntaxNode.Body.ChildNodes().OfType<ForEachStatementSyntax>();
    }
}
