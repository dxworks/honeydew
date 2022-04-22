using Honeydew.Extractors.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Honeydew.Extractors.CSharp.Visitors.Setters;

public class AttributeSetterVisitor : CompositeVisitor, ICSharpClassVisitor, ICSharpDelegateVisitor,
    ICSharpMethodVisitor, ICSharpConstructorVisitor, ICSharpFieldVisitor, ICSharpPropertyVisitor,
    ICSharpParameterVisitor, ICSharpMethodAccessorVisitor, ICSharpGenericParameterVisitor, ICSharpDestructorVisitor,
    ICSharpEnumVisitor, ICSharpEnumLabelVisitor
{
    public AttributeSetterVisitor(IEnumerable<IAttributeVisitor> visitors) : base(visitors)
    {
    }

    public IMembersClassType Visit(TypeDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IMembersClassType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "type");

        return modelType;
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        foreach (var attributeType in ExtractAttributesFromMethod(syntaxNode, semanticModel))
        {
            if (attributeType.Target == "return")
            {
                modelType.ReturnValue.Attributes.Add(attributeType);
            }
            else
            {
                modelType.Attributes.Add(attributeType);
            }
        }

        return modelType;
    }

    public IAccessorType Visit(AccessorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IAccessorType modelType)
    {
        foreach (var attributeType in ExtractAttributesFromMethod(syntaxNode, semanticModel))
        {
            if (attributeType.Target == "return")
            {
                modelType.ReturnValue.Attributes.Add(attributeType);
            }
            else
            {
                modelType.Attributes.Add(attributeType);
            }
        }

        return modelType;
    }

    public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IConstructorType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "method");

        return modelType;
    }

    public IDestructorType Visit(DestructorDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDestructorType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "method");

        return modelType;
    }

    public IList<IFieldType> Visit(BaseFieldDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IList<IFieldType> modelType)
    {
        foreach (var fieldType in modelType)
        {
            ExtractAttributes(syntaxNode, semanticModel, fieldType, "field");
        }

        return modelType;
    }

    public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "property");

        return modelType;
    }

    public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IDelegateType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "type");

        return modelType;
    }

    public IEnumType Visit(EnumDeclarationSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "type");

        return modelType;
    }

    public IEnumLabelType Visit(EnumMemberDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IEnumLabelType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "field");

        return modelType;
    }

    public IParameterType Visit(ParameterSyntax syntaxNode, SemanticModel semanticModel, IParameterType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "param");

        return modelType;
    }

    public IGenericParameterType Visit(TypeParameterSyntax syntaxNode, SemanticModel semanticModel,
        IGenericParameterType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "param");

        return modelType;
    }

    private void ExtractAttributes(SyntaxNode syntaxNode, SemanticModel semanticModel, ITypeWithAttributes modelType,
        string target)
    {
        foreach (var attributeListSyntax in syntaxNode.ChildNodes().OfType<AttributeListSyntax>())
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                IAttributeType attributeModel = new AttributeModel();

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpAttributeVisitor extractionVisitor)
                        {
                            attributeModel = extractionVisitor.Visit(attributeSyntax, semanticModel, attributeModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Attribute Visitor because {e}", LogLevels.Warning);
                    }
                }

                attributeModel.Target = target;

                modelType.Attributes.Add(attributeModel);
            }
        }
    }

    private IEnumerable<IAttributeType> ExtractAttributesFromMethod(SyntaxNode syntaxNode, SemanticModel semanticModel)
    {
        var attributes = new List<IAttributeType>();

        foreach (var attributeListSyntax in syntaxNode.ChildNodes().OfType<AttributeListSyntax>())
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                IAttributeType attributeModel = new AttributeModel();

                attributeModel.Target = "method";

                foreach (var visitor in GetContainedVisitors())
                {
                    try
                    {
                        if (visitor is ICSharpAttributeVisitor extractionVisitor)
                        {
                            attributeModel = extractionVisitor.Visit(attributeSyntax, semanticModel, attributeModel);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Could not extract from Attribute Visitor because {e}", LogLevels.Warning);
                    }
                }

                attributes.Add(attributeModel);
            }
        }

        return attributes;
    }
}
