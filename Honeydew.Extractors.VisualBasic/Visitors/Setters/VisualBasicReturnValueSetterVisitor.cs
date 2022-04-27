using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public partial class VisualBasicReturnValueSetterVisitor :
    CompositeVisitor<IReturnValueType>,
    IReturnValueSetterVisitor<DelegateStatementSyntax, SemanticModel, TypeSyntax, IDelegateType>
    // IReturnValueSetterVisitor<MethodBlockSyntax, SemanticModel, TypeSyntax, IMethodType>,
    // IReturnValueSetterVisitor<ArrowExpressionClauseSyntax, SemanticModel, TypeSyntax, IAccessorMethodType>,
    // IReturnValueSetterVisitor<LocalFunctionStatementSyntax, SemanticModel, TypeSyntax, IMethodTypeWithLocalFunctions>
{
    public VisualBasicReturnValueSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IReturnValueType>> visitors)
        : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IReturnValueType CreateWrappedType() => new VisualBasicReturnValueModel();

    public IEnumerable<TypeSyntax> GetWrappedSyntaxNodes(DelegateStatementSyntax syntaxNode)
    {
        yield return syntaxNode.AsClause.Type;
    }

    // public IEnumerable<TypeSyntax> GetWrappedSyntaxNodes(MethodBlockSyntax syntaxNode)
    // {
    //     yield return syntaxNode.BlockStatement;
    // }

    // public IEnumerable<TypeSyntax> GetWrappedSyntaxNodes(ArrowExpressionClauseSyntax syntaxNode)
    // {
    //     var basePropertyDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BasePropertyDeclarationSyntax>();
    //     if (basePropertyDeclarationSyntax != null)
    //     {
    //         yield return basePropertyDeclarationSyntax.Type;
    //     }
    // }
    //
    // public IEnumerable<TypeSyntax> GetWrappedSyntaxNodes(LocalFunctionStatementSyntax syntaxNode)
    // {
    //     yield return syntaxNode.ReturnType;
    // }
}
