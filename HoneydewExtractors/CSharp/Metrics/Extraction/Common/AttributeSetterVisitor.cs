using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Destructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common;

[Obfuscation]
public class AttributeSetterVisitor : CompositeVisitor, ICSharpClassVisitor, ICSharpDelegateVisitor,
    ICSharpMethodVisitor, ICSharpConstructorVisitor, ICSharpFieldVisitor, ICSharpPropertyVisitor,
    ICSharpParameterVisitor, ICSharpMethodAccessorVisitor, ICSharpGenericParameterVisitor, ICSharpDestructorVisitor
{
    public AttributeSetterVisitor(IEnumerable<IAttributeVisitor> visitors) : base(visitors)
    {
    }

    public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, SemanticModel semanticModel, IClassType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "class");

        return modelType;
    }

    public IMethodType Visit(MethodDeclarationSyntax syntaxNode, SemanticModel semanticModel, IMethodType modelType)
    {
        foreach (var attributeType in ExtractAttributesFromMethod(syntaxNode, semanticModel))
        {
            if (attributeType.TargetType == "return")
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
            if (attributeType.TargetType == "return")
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
        ExtractAttributes(syntaxNode, semanticModel, modelType, "constructor");

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
        ExtractAttributes(syntaxNode, semanticModel, modelType, "delegate");

        return modelType;
    }

    public IParameterType Visit(ParameterSyntax syntaxNode, SemanticModel semanticModel, IParameterType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "parameter");

        return modelType;
    }

    public IGenericParameterType Visit(TypeParameterSyntax syntaxNode, SemanticModel semanticModel,
        IGenericParameterType modelType)
    {
        ExtractAttributes(syntaxNode, semanticModel, modelType, "parameter");

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

                attributeModel.TargetType = target;

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

                attributeModel.TargetType = "method";

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
