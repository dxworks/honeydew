using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicMethodSetterVisitor :
    CompositeVisitor<IMethodType>,
    IMethodSetterClassVisitor<ClassBlockSyntax, SemanticModel, MethodStatementSyntax>,
    IMethodSetterClassVisitor<InterfaceBlockSyntax, SemanticModel, MethodStatementSyntax>,
    IMethodSetterClassVisitor<StructureBlockSyntax, SemanticModel, MethodStatementSyntax>

{
    public VisualBasicMethodSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IMethodType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IMethodType CreateWrappedType() => new VisualBasicMethodModel();

    public IEnumerable<MethodStatementSyntax> GetWrappedSyntaxNodes(ClassBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<MethodStatementSyntax>();
    }

    public IEnumerable<MethodStatementSyntax> GetWrappedSyntaxNodes(InterfaceBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<MethodStatementSyntax>();
    }

    public IEnumerable<MethodStatementSyntax> GetWrappedSyntaxNodes(StructureBlockSyntax syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<MethodStatementSyntax>();
    }
}
