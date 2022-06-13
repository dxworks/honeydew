using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Extraction;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicGenericParameterSetterVisitor :
    CompositeVisitor<IGenericParameterType>,
    IGenericParameterSetterVisitor<ClassBlockSyntax, SemanticModel, TypeParameterSyntax, IMembersClassType>,
    IGenericParameterSetterVisitor<StructureBlockSyntax, SemanticModel, TypeParameterSyntax, IMembersClassType>,
    IGenericParameterSetterVisitor<InterfaceBlockSyntax, SemanticModel, TypeParameterSyntax, IMembersClassType>,
    IGenericParameterSetterVisitor<DelegateStatementSyntax, SemanticModel, TypeParameterSyntax, IDelegateType>,
    IGenericParameterSetterVisitor<MethodStatementSyntax, SemanticModel, TypeParameterSyntax, IMethodType>
{
    public VisualBasicGenericParameterSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IGenericParameterType>> visitors) : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IGenericParameterType CreateWrappedType() => new VisualBasicGenericParameterModel();

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(ClassBlockSyntax syntaxNode)
    {
        return GetTypeParameterSyntaxNodes(syntaxNode);
    }

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(InterfaceBlockSyntax syntaxNode)
    {
        return GetTypeParameterSyntaxNodes(syntaxNode);
    }

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(StructureBlockSyntax syntaxNode)
    {
        return GetTypeParameterSyntaxNodes(syntaxNode);
    }

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(DelegateStatementSyntax syntaxNode)
    {
        return GetTypeParameterSyntaxNodes(syntaxNode);
    }

    public IEnumerable<TypeParameterSyntax> GetWrappedSyntaxNodes(MethodStatementSyntax syntaxNode)
    {
        return GetTypeParameterSyntaxNodes(syntaxNode);
    }

    private static IEnumerable<TypeParameterSyntax> GetTypeParameterSyntaxNodes(SyntaxNode syntaxNode)
    {
        return syntaxNode.DescendantNodes().OfType<TypeParameterListSyntax>()
            .SelectMany(listSyntax => listSyntax.Parameters);
    }
}
