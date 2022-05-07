using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Extractors.Visitors.Setters;
using Honeydew.Logging;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Honeydew.Extractors.VisualBasic.Visitors.Setters;

public class VisualBasicAttributeSetterVisitor :
    CompositeVisitor<IAttributeType>,
    IAttributeSetterVisitor<ClassBlockSyntax, SemanticModel, AttributeSyntax, IMembersClassType>,
    IAttributeSetterVisitor<StructureBlockSyntax, SemanticModel, AttributeSyntax, IMembersClassType>,
    IAttributeSetterVisitor<InterfaceBlockSyntax, SemanticModel, AttributeSyntax, IMembersClassType>,
    IAttributeSetterVisitor<DelegateStatementSyntax, SemanticModel, AttributeSyntax, IDelegateType>,
    IAttributeSetterVisitor<EnumBlockSyntax, SemanticModel, AttributeSyntax, IEnumType>,
    IAttributeSetterVisitor<EnumMemberDeclarationSyntax, SemanticModel, AttributeSyntax, IEnumLabelType>,
    IAttributeSetterVisitor<MethodStatementSyntax, SemanticModel, AttributeSyntax, IMethodType>,
    IAttributeSetterVisitor<ConstructorBlockSyntax, SemanticModel, AttributeSyntax, IConstructorType>,
    IAttributeSetterVisitor<MethodBlockSyntax, SemanticModel, AttributeSyntax, IDestructorType>,
    IAttributeSetterVisitor<AccessorBlockSyntax, SemanticModel, AttributeSyntax, IAccessorMethodType>,
    IAttributeSetterVisitor<ModifiedIdentifierSyntax, SemanticModel, AttributeSyntax, IFieldType>,
    IAttributeSetterVisitor<PropertyStatementSyntax, SemanticModel, AttributeSyntax, IPropertyType>,
    IAttributeSetterVisitor<ParameterSyntax, SemanticModel, AttributeSyntax, IParameterType>,
    IAttributeSetterVisitor<TypeParameterSyntax, SemanticModel, AttributeSyntax, IGenericParameterType>,
    IAttributeSetterVisitor<ReturnValueModel, SemanticModel, AttributeSyntax, IReturnValueType>
{
    public VisualBasicAttributeSetterVisitor(ILogger compositeLogger,
        IEnumerable<ITypeVisitor<IAttributeType>> visitors) :
        base(compositeLogger, visitors)
    {
    }

    public ILogger Logger => CompositeLogger;

    public IAttributeType CreateWrappedType() => new VisualBasicAttributeModel();

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ClassBlockSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "type");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(StructureBlockSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "type");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(InterfaceBlockSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "type");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(DelegateStatementSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "type");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(EnumBlockSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "type");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(EnumMemberDeclarationSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "field");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(MethodStatementSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ConstructorBlockSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(MethodBlockSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(AccessorBlockSyntax syntaxNode)
    {
        return GetAttributeSyntaxNodes(syntaxNode, "method");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ModifiedIdentifierSyntax syntaxNode)
    {
        var baseFieldDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<FieldDeclarationSyntax>();

        return baseFieldDeclarationSyntax is null
            ? Enumerable.Empty<AttributeSyntax>()
            : GetAttributeSyntaxNodes(baseFieldDeclarationSyntax, "field");
    }

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(PropertyStatementSyntax syntaxNode)
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

    public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(ReturnValueModel syntaxNode)
    {
        if (syntaxNode.ReturnType?.Parent is null)
        {
            return Enumerable.Empty<AttributeSyntax>();
        }

        return syntaxNode.ReturnType.Parent.DescendantNodes()
            .OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes)
            .Where(attributeSyntaxList =>
                attributeSyntaxList.Target is not null &&
                "return" == attributeSyntaxList.Target.ToString().TrimEnd(':'));
    }

    // public IEnumerable<AttributeSyntax> GetWrappedSyntaxNodes(AccessorReturnValue accessorReturnValue)
    // {
    //     if (accessorReturnValue.ReturnType.Parent is BasePropertyDeclarationSyntax basePropertyDeclarationSyntax)
    //     {
    //         var accessor =
    //             basePropertyDeclarationSyntax.AccessorList?.Accessors.FirstOrDefault(a =>
    //                 a.Keyword.ToString() == accessorReturnValue.Type);
    //         if (accessor != null)
    //         {
    //             return accessor.ChildNodes()
    //                 .OfType<AttributeListSyntax>()
    //                 .Where(attributeSyntaxList =>
    //                     attributeSyntaxList.Target is not null &&
    //                     "return" == attributeSyntaxList.Target.ToString().TrimEnd(':'))
    //                 .SelectMany(listSyntax => listSyntax.Attributes);
    //         }
    //     }
    //
    //     return Enumerable.Empty<AttributeSyntax>();
    // }

    private IEnumerable<AttributeSyntax> GetAttributeSyntaxNodes(SyntaxNode syntaxNode, string allowedTarget)
    {
        return syntaxNode
            .DescendantNodes()
            .OfType<AttributeListSyntax>()
            .SelectMany(listSyntax => listSyntax.Attributes)
            .Where(attributeSyntaxList =>
                attributeSyntaxList.Target is null ||
                allowedTarget == attributeSyntaxList.Target.ToString().TrimEnd(':'));
    }
}
