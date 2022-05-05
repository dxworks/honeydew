using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public partial class VisualBasicLocalVariablesTypeSetterVisitor :
    ILocalVariablesTypeSetterVisitor<ConstructorBlockSyntax, SemanticModel, ModifiedIdentifierSyntax, IConstructorType>
{
    public IConstructorType Visit(ConstructorBlockSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        var missingLocalVariablesCount = 0;

        // variable declarator
        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<ConstructorBlockSyntax, SemanticModel, ModifiedIdentifierSyntax,
                IConstructorType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<ConstructorBlockSyntax, SemanticModel, ModifiedIdentifierSyntax
                    , IConstructorType>)this)
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
        ISetterVisitor<ConstructorBlockSyntax, SemanticModel, IConstructorType, ModifiedIdentifierSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(ConstructorBlockSyntax syntaxNode)
    {
        return GetVariableDeclaratorSyntaxNodes(syntaxNode)
            .SelectMany(declaratorSyntax => declaratorSyntax.Names);
    }
}
