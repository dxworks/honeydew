using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicParameterSetterVisitor :
        CompositeVisitor<IParameterType>,
        IParameterSetterVisitor<DelegateStatementSyntax, SemanticModel, ParameterSyntax, IDelegateType>,
        IParameterSetterVisitor<MethodStatementSyntax, SemanticModel, ParameterSyntax, IMethodType>,
        IParameterSetterVisitor<ConstructorBlockSyntax, SemanticModel, ParameterSyntax, IConstructorType>,
        IParameterSetterVisitor<AccessorBlockSyntax, SemanticModel, ParameterSyntax, IAccessorMethodType>
{
    public VisualBasicParameterSetterVisitor(ILogger logger, IEnumerable<ITypeVisitor<IParameterType>> visitors) : base(
        logger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IParameterType CreateWrappedType() => new VisualBasicParameterModel();

    public IEnumerable<ParameterSyntax> GetWrappedSyntaxNodes(DelegateStatementSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<ParameterListSyntax>()
            .SelectMany(syntax => syntax.Parameters);
    }

    public IEnumerable<ParameterSyntax> GetWrappedSyntaxNodes(MethodStatementSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<ParameterListSyntax>()
            .SelectMany(syntax => syntax.Parameters);
    }

    public IEnumerable<ParameterSyntax> GetWrappedSyntaxNodes(ConstructorBlockSyntax syntaxNode)
    {
        return syntaxNode.BlockStatement.ChildNodes().OfType<ParameterListSyntax>()
            .SelectMany(syntax => syntax.Parameters);
    }

    public IEnumerable<ParameterSyntax> GetWrappedSyntaxNodes(AccessorBlockSyntax syntaxNode)
    {
        return syntaxNode.AccessorStatement.ChildNodes().OfType<ParameterListSyntax>()
            .SelectMany(syntax => syntax.Parameters);
    }
}
