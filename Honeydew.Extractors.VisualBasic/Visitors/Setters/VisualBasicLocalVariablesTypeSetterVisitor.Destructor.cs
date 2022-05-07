using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public partial class VisualBasicLocalVariablesSetterVisitor :
    ILocalVariablesTypeSetterVisitor<MethodBlockSyntax, SemanticModel, ModifiedIdentifierSyntax, IDestructorType>
{
    public IDestructorType Visit(MethodBlockSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        var missingLocalVariablesCount = 0;

        var variableDeclaratorSyntaxNodes =
            ((ILocalVariablesTypeSetterVisitor<MethodBlockSyntax, SemanticModel, ModifiedIdentifierSyntax,
                IDestructorType>)this)
            .GetWrappedSyntaxNodes(syntaxNode);

        foreach (var wrappedSyntaxNode in variableDeclaratorSyntaxNodes)
        {
            var localVariableType =
                ((ILocalVariablesTypeSetterVisitor<MethodBlockSyntax, SemanticModel, ModifiedIdentifierSyntax,
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

    IEnumerable<ModifiedIdentifierSyntax>
        ISetterVisitor<MethodBlockSyntax, SemanticModel, IDestructorType, ModifiedIdentifierSyntax,
            ILocalVariableType>.GetWrappedSyntaxNodes(MethodBlockSyntax syntaxNode)
    {
        return GetVariableDeclaratorSyntaxNodes(syntaxNode)
            .SelectMany(declaratorSyntax => declaratorSyntax.Names);
    }
}
