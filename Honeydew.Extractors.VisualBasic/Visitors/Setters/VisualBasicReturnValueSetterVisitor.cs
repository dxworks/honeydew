using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public record ReturnValueModel(string Type, TypeSyntax? ReturnType);

public class VisualBasicReturnValueSetterVisitor :
    CompositeVisitor<IReturnValueType>,
    IReturnValueSetterVisitor<DelegateStatementSyntax, SemanticModel, ReturnValueModel, IDelegateType>,
    IReturnValueSetterVisitor<MethodBlockSyntax, SemanticModel, ReturnValueModel, IMethodType>,
    IReturnValueSetterVisitor<MethodStatementSyntax, SemanticModel, ReturnValueModel, IMethodType>,
    IReturnValueSetterVisitor<AccessorBlockSyntax, SemanticModel, ReturnValueModel, IAccessorMethodType>
{
    public VisualBasicReturnValueSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IReturnValueType>> visitors)
        : base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IReturnValueType CreateWrappedType() => new VisualBasicReturnValueModel();

    public IEnumerable<ReturnValueModel> GetWrappedSyntaxNodes(DelegateStatementSyntax syntaxNode)
    {
        yield return new ReturnValueModel("return", syntaxNode.AsClause?.Type);
    }

    public IEnumerable<ReturnValueModel> GetWrappedSyntaxNodes(MethodBlockSyntax syntaxNode)
    {
        yield return new ReturnValueModel("return", syntaxNode.SubOrFunctionStatement.AsClause?.Type);
    }

    public IEnumerable<ReturnValueModel> GetWrappedSyntaxNodes(MethodStatementSyntax syntaxNode)
    {
        yield return new ReturnValueModel("return", syntaxNode.AsClause?.Type);
    }

    public IEnumerable<ReturnValueModel> GetWrappedSyntaxNodes(AccessorBlockSyntax syntaxNode)
    {
        var propertyBlockSyntax = syntaxNode.GetParentDeclarationSyntax<PropertyBlockSyntax>();
        if (propertyBlockSyntax != null)
        {
            var accessorType = syntaxNode.AccessorStatement.AccessorKeyword.ToString();
            var returnType = accessorType != "Get"
                ? null
                : (propertyBlockSyntax.PropertyStatement?.AsClause as SimpleAsClauseSyntax)?.Type;
            yield return new ReturnValueModel(accessorType, returnType);
        }
    }
}
