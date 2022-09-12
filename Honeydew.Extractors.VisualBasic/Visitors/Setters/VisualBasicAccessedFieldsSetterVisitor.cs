using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicAccessedFieldsSetterVisitor :
    CompositeVisitor<AccessedField>,
    IAccessedFieldsSetterVisitor<MethodStatementSyntax, SemanticModel, ExpressionSyntax, IMethodType>,
    IAccessedFieldsSetterVisitor<ConstructorBlockSyntax, SemanticModel, ExpressionSyntax, IConstructorType>,
    IAccessedFieldsSetterVisitor<MethodBlockSyntax, SemanticModel, ExpressionSyntax, IDestructorType>,
    IAccessedFieldsSetterVisitor<AccessorBlockSyntax, SemanticModel, ExpressionSyntax, IAccessorMethodType>
{
    public VisualBasicAccessedFieldsSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<AccessedField>> visitors)
        : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public AccessedField CreateWrappedType() => new();

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(MethodStatementSyntax syntaxNode)
    {
        if (syntaxNode.Parent is MethodBlockSyntax methodBlockSyntax)
        {
            var descendantNodes = methodBlockSyntax.DescendantNodes().ToList();
            return GetPossibleAccessFields(descendantNodes);
        }
        
        return Enumerable.Empty<ExpressionSyntax>();
    }

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(ConstructorBlockSyntax syntaxNode)
    {
        var descendantNodes = syntaxNode.DescendantNodes().ToList();
        return GetPossibleAccessFields(descendantNodes);
    }

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(MethodBlockSyntax syntaxNode)
    {
        var descendantNodes = syntaxNode.DescendantNodes().ToList();
        return GetPossibleAccessFields(descendantNodes);
    }

    public IEnumerable<ExpressionSyntax> GetWrappedSyntaxNodes(AccessorBlockSyntax syntaxNode)
    {
        var descendantNodes = syntaxNode.DescendantNodes().ToList();
        return GetPossibleAccessFields(descendantNodes);
    }

    private static IEnumerable<ExpressionSyntax> GetPossibleAccessFields(List<SyntaxNode> descendantNodes)
    {
        return descendantNodes.OfType<IdentifierNameSyntax>();
    }
}
