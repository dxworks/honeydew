using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public partial class VisualBasicLocalVariablesSetterVisitor :
    ILocalVariablesTypeSetterVisitor<MethodStatementSyntax, SemanticModel, ModifiedIdentifierSyntax, IMethodType>
{
    public IMethodType Visit(MethodStatementSyntax syntaxNode, SemanticModel semanticModel,
        IMethodType modelType)
    {
        var missingLocalVariablesCount = 0;

        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<MethodStatementSyntax, SemanticModel, ModifiedIdentifierSyntax,
                IMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<MethodStatementSyntax, SemanticModel, ModifiedIdentifierSyntax,
                    IMethodType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType,
                ref missingLocalVariablesCount);
        }

        if (missingLocalVariablesCount > 0)
        {
            Logger.Log($"Could not set {missingLocalVariablesCount} local variables", LogLevels.Warning);
        }

        return modelType;
    }

    public IEnumerable<ModifiedIdentifierSyntax> GetWrappedSyntaxNodes(MethodStatementSyntax syntaxNode)
    {
        var methodBlockSyntax = syntaxNode.GetParentDeclarationSyntax<MethodBlockSyntax>();
        if (methodBlockSyntax is null)
        {
            return Enumerable.Empty<ModifiedIdentifierSyntax>();
        }

        return GetVariableDeclaratorSyntaxNodes(methodBlockSyntax)
            .SelectMany(declaratorSyntax => declaratorSyntax.Names);
    }
}
