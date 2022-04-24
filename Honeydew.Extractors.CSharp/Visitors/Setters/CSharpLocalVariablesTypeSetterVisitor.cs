using Honeydew.Extractors.Visitors;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public partial class CSharpLocalVariablesTypeSetterVisitor : CompositeVisitor<ILocalVariableType>

{
    public CSharpLocalVariablesTypeSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<ILocalVariableType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public ILocalVariableType CreateWrappedType() => new LocalVariableModel();

    private IEnumerable<VariableDeclaratorSyntax> GetVariableDeclaratorSyntaxNodes(SyntaxNode syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<VariableDeclaratorSyntax>();
    }

    private IEnumerable<DeclarationPatternSyntax> GetDeclarationPatternsSyntaxNodes(SyntaxNode syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<DeclarationPatternSyntax>();
    }

    private IEnumerable<ForEachStatementSyntax> GetForEachStatementsSyntaxNodes(SyntaxNode syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<ForEachStatementSyntax>();
    }

    private static void AddLocalVariable(ITypeWithLocalVariables typeWithLocalVariables,
        ILocalVariableType? localVariableType, ref int missingLocalVariablesCount)
    {
        if (localVariableType?.Type != null && !string.IsNullOrEmpty(localVariableType.Type.Name) &&
            localVariableType.Type.Name != CSharpConstants.VarIdentifier)
        {
            typeWithLocalVariables.LocalVariableTypes.Add(localVariableType);
        }
        else
        {
            Interlocked.Increment(ref missingLocalVariablesCount);
        }
    }
}
