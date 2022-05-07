using Honeydew.Extractors.Visitors;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public partial class VisualBasicLocalVariablesSetterVisitor : CompositeVisitor<ILocalVariableType>
{
    public VisualBasicLocalVariablesSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<ILocalVariableType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public ILocalVariableType CreateWrappedType() => new VisualBasicLocalVariableModel();

    private IEnumerable<VariableDeclaratorSyntax> GetVariableDeclaratorSyntaxNodes(SyntaxNode syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<VariableDeclaratorSyntax>();
    }

    private static void AddLocalVariable(ITypeWithLocalVariables typeWithLocalVariables,
        ILocalVariableType? localVariableType, ref int missingLocalVariablesCount)
    {
        if (localVariableType?.Type != null && !string.IsNullOrEmpty(localVariableType.Type.Name) &&
            localVariableType.Type.Name != VisualBasicConstants.VarIdentifier)
        {
            typeWithLocalVariables.LocalVariableTypes.Add(localVariableType);
        }
        else
        {
            Interlocked.Increment(ref missingLocalVariablesCount);
        }
    }
}
