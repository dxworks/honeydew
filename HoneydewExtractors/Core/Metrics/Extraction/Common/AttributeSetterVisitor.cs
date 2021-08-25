﻿using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Attributes;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewExtractors.Core.Metrics.Visitors.Constructors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.Core.Metrics.Visitors.Methods;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Common
{
    public class AttributeSetterVisitor : CompositeVisitor, ICSharpClassVisitor, ICSharpDelegateVisitor,
        ICSharpMethodVisitor, ICSharpConstructorVisitor, ICSharpFieldVisitor, ICSharpPropertyVisitor
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
            ExtractAttributes(syntaxNode, modelType, "method");

            return modelType;
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

        private void ExtractAttributes(SyntaxNode syntaxNode, ITypeWithAttributes modelType, string target)
        {
            foreach (var attributeSyntax in syntaxNode.DescendantNodes().OfType<AttributeSyntax>())
            {
                IAttributeType attributeModel = new AttributeModel();

                attributeModel.Target = target;

                foreach (var visitor in GetContainedVisitors())
                {
                    if (visitor is ICSharpAttributeVisitor extractionVisitor)
                    {
                        attributeModel = extractionVisitor.Visit(attributeSyntax, attributeModel);
                    }
                }

                modelType.Attributes.Add(attributeModel);
            }
        }
    }
}