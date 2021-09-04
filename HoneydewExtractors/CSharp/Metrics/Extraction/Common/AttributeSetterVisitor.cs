using System;
using System.Collections.Generic;
using System.Linq;
using HoneydewCore.Logging;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Parameters;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Common
{
    public class AttributeSetterVisitor : CompositeVisitor, ICSharpClassVisitor, ICSharpDelegateVisitor,
        ICSharpMethodVisitor, ICSharpConstructorVisitor, ICSharpFieldVisitor, ICSharpPropertyVisitor,
        ICSharpParameterVisitor, ICSharpMethodAccessorVisitor, ICSharpGenericParameterVisitor
    {
        public AttributeSetterVisitor(IEnumerable<IAttributeVisitor> visitors) : base(visitors)
        {
        }

        public IClassType Visit(BaseTypeDeclarationSyntax syntaxNode, IClassType modelType)
        {
            ExtractAttributes(syntaxNode, modelType, "class");

            return modelType;
        }

        public IMethodType Visit(MethodDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            return ExtractAttributesFromMethod(syntaxNode, modelType);
        }

        public IMethodType Visit(AccessorDeclarationSyntax syntaxNode, IMethodType modelType)
        {
            return ExtractAttributesFromMethod(syntaxNode, modelType);
        }

        public IConstructorType Visit(ConstructorDeclarationSyntax syntaxNode, IConstructorType modelType)
        {
            ExtractAttributes(syntaxNode, modelType, "constructor");

            return modelType;
        }

        public IList<IFieldType> Visit(BaseFieldDeclarationSyntax syntaxNode, IList<IFieldType> modelType)
        {
            foreach (var fieldType in modelType)
            {
                ExtractAttributes(syntaxNode, fieldType, "field");
            }

            return modelType;
        }

        public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, IPropertyType modelType)
        {
            ExtractAttributes(syntaxNode, modelType, "property");

            return modelType;
        }

        public IDelegateType Visit(DelegateDeclarationSyntax syntaxNode, IDelegateType modelType)
        {
            ExtractAttributes(syntaxNode, modelType, "delegate");

            return modelType;
        }

        public IParameterType Visit(ParameterSyntax syntaxNode, IParameterType modelType)
        {
            ExtractAttributes(syntaxNode, modelType, "parameter");

            return modelType;
        }

        public IGenericParameterType Visit(TypeParameterSyntax syntaxNode, IGenericParameterType modelType)
        {
            ExtractAttributes(syntaxNode, modelType, "parameter");

            return modelType;
        }

        private void ExtractAttributes(SyntaxNode syntaxNode, ITypeWithAttributes modelType, string target)
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
                                attributeModel = extractionVisitor.Visit(attributeSyntax, attributeModel);
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

        private IMethodType ExtractAttributesFromMethod(SyntaxNode syntaxNode, IMethodType modelType)
        {
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
                                attributeModel = extractionVisitor.Visit(attributeSyntax, attributeModel);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Log($"Could not extract from Attribute Visitor because {e}", LogLevels.Warning);
                        }
                    }

                    if (attributeModel.Target == "return")
                    {
                        modelType.ReturnValue.Attributes.Add(attributeModel);
                    }
                    else
                    {
                        modelType.Attributes.Add(attributeModel);
                    }
                }
            }

            return modelType;
        }
    }
}
