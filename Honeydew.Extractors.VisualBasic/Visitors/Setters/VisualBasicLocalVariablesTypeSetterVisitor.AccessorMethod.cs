using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public partial class VisualBasicLocalVariablesSetterVisitor :
    ILocalVariablesTypeSetterVisitor<AccessorBlockSyntax, SemanticModel, ModifiedIdentifierSyntax,
        IAccessorMethodType>
{
    public IAccessorMethodType Visit(AccessorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorMethodType modelType)
    {
        var missingLocalVariablesCount = 0;

        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<AccessorBlockSyntax, SemanticModel, ModifiedIdentifierSyntax,
                IAccessorMethodType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<AccessorBlockSyntax, SemanticModel,
                    ModifiedIdentifierSyntax,
                    IAccessorMethodType>)this)
                .ApplyContainedVisitors(wrappedSyntaxNode, CreateWrappedType(), semanticModel);

            AddLocalVariable(modelType, localVariableType, ref missingLocalVariablesCount);
        }

        if (missingLocalVariablesCount > 0)
        {
            Logger.Log($"Could not set {missingLocalVariablesCount} local variables", LogLevels.Warning);
        }

        return modelType;
    }

    IEnumerable<ModifiedIdentifierSyntax>
        ISetterVisitor<AccessorBlockSyntax, SemanticModel, IAccessorMethodType, ModifiedIdentifierSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(AccessorBlockSyntax syntaxNode)
    {
        return GetVariableDeclaratorSyntaxNodes(syntaxNode)
            .SelectMany(declaratorSyntax => declaratorSyntax.Names);
    }
}
