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
    IAttributeSetterVisitor<BaseFieldDeclarationSyntax, SemanticModel, AttributeSyntax, IFieldType>,
    IAttributeSetterVisitor<BasePropertyDeclarationSyntax, SemanticModel, AttributeSyntax, IPropertyType>,
    IAttributeSetterVisitor<ParameterSyntax, SemanticModel, AttributeSyntax, IParameterType>,
    IAttributeSetterVisitor<TypeParameterSyntax, SemanticModel, AttributeSyntax, IGenericParameterType>
{
    public CSharpAttributeSetterVisitor(ILogger compositeLogger, IEnumerable<ITypeVisitor<IAttributeType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IAttributeType CreateWrappedType() => new AttributeModel();

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(TypeDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(DelegateDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(EnumDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(EnumMemberDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(MethodDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ConstructorDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(DestructorDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(AccessorDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(BaseFieldDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(BasePropertyDeclarationSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ParameterSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(TypeParameterSyntax syntaxNode)
    {
        return syntaxNode.ChildNodes().OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes);
    }
}
