using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Models;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class CSharpAttributeSetterVisitor :
    CompositeVisitor<IAttributeType>,
    IAttributeSetterVisitor<TypeDeclarationSyntax, SemanticModel, AttributeSyntax, IMembersClassType>,
    IAttributeSetterVisitor<DelegateDeclarationSyntax, SemanticModel, AttributeSyntax, IDelegateType>,
    IAttributeSetterVisitor<EnumDeclarationSyntax, SemanticModel, AttributeSyntax, IEnumType>,
    IAttributeSetterVisitor<EnumMemberDeclarationSyntax, SemanticModel, AttributeSyntax, IEnumLabelType>,
    IAttributeSetterVisitor<MethodDeclarationSyntax, SemanticModel, AttributeSyntax, IMethodType>,
    IAttributeSetterVisitor<ConstructorDeclarationSyntax, SemanticModel, AttributeSyntax, IConstructorType>,
    IAttributeSetterVisitor<DestructorDeclarationSyntax, SemanticModel, AttributeSyntax, IDestructorType>,
    IAttributeSetterVisitor<AccessorDeclarationSyntax, SemanticModel, AttributeSyntax, IAccessorMethodType>,
    IAttributeSetterVisitor<ArrowExpressionClauseSyntax, SemanticModel, AttributeSyntax, IAccessorMethodType>,
    IAttributeSetterVisitor<VariableDeclaratorSyntax, SemanticModel, AttributeSyntax, IFieldType>,
    IAttributeSetterVisitor<BasePropertyDeclarationSyntax, SemanticModel, AttributeSyntax, IPropertyType>,
    IAttributeSetterVisitor<ParameterSyntax, SemanticModel, AttributeSyntax, IParameterType>,
    IAttributeSetterVisitor<TypeParameterSyntax, SemanticModel, AttributeSyntax, IGenericParameterType>,
    IAttributeSetterVisitor<TypeSyntax, SemanticModel, AttributeSyntax, IReturnValueType>,
    IAttributeSetterVisitor<AccessorReturnValue, SemanticModel, AttributeSyntax, IReturnValueType>
{
    public CSharpAttributeSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IAttributeType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IAttributeType CreateWrappedType() => new AttributeModel();

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "type");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(DelegateDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "type");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(EnumDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "type");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(EnumMemberDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "field");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(DestructorDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ArrowExpressionClauseSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(VariableDeclaratorSyntax syntaxNode)
    {
        var baseFieldDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<BaseFieldDeclarationSyntax>();

        return baseFieldDeclarationSyntax is null
            ? Enumerable.Empty<AttributeSyntax>()
            : GetAttributeSyntaxNodes(baseFieldDeclarationSyntax, "field");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(BasePropertyDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "property");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ParameterSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "param");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(TypeParameterSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "param");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(TypeSyntax syntaxNode)
    {
        if (syntaxNode.Parent is null)
        {
            return Enumerable.Empty<AttributeSyntax>();
        }

        return syntaxNode.Parent.ChildNodes()
            .OfType<AttributeListSyntax>()
            .Where(attributeSyntaxList =>
                attributeSyntaxList.Target is not null &&
                "return" == attributeSyntaxList.Target.ToString().TrimEnd(':'))
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(AccessorReturnValue accessorReturnValue)
    {
        if (accessorReturnValue.ReturnType.Parent is BasePropertyDeclarationSyntax basePropertyDeclarationSyntax)
        {
            var accessor =
                basePropertyDeclarationSyntax.AccessorList?.Accessors.FirstOrDefault(a =>
                    a.Keyword.ToString() == accessorReturnValue.Type);
            if (accessor != null)
            {
                return accessor.ChildNodes()
                    .OfType<AttributeListSyntax>()
                    .Where(attributeSyntaxList =>
                        attributeSyntaxList.Target is not null &&
                        "return" == attributeSyntaxList.Target.ToString().TrimEnd(':'))
                    .SelectMany(listSyntax => listSyntax.Attributes);
            }
        }

        return Enumerable.Empty<AttributeSyntax>();
    }

    private IEnumerable<AttributeSyntax> GetAttributeSyntaxNodes(SyntaxNode syntaxNode, string allowedTarget)
    {
        return syntaxNode
            .ChildNodes()
            .OfType<AttributeListSyntax>()
            .Where(attributeSyntaxList =>
                attributeSyntaxList.Target is null ||
                allowedTarget == attributeSyntaxList.Target.ToString().TrimEnd(':'))
            .SelectMany(listSyntax => listSyntax.Attributes);
    }
}
