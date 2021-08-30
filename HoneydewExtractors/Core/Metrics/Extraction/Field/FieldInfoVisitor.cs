﻿using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Field
{
    public class FieldInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor,
        ICSharpFieldVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IList<IFieldType> Visit(BaseFieldDeclarationSyntax syntaxNode, IList<IFieldType> modelType)
        {
            var allModifiers = syntaxNode.Modifiers.ToString();
            var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
            var modifier = allModifiers;

            var containingClass = "";
            if (syntaxNode.Parent is BaseTypeDeclarationSyntax classDeclarationSyntax)
            {
                containingClass = CSharpHelperMethods.GetFullName(classDeclarationSyntax);
            }

            CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

            var typeName = CSharpHelperMethods.GetFullName(syntaxNode.Declaration);

            var isEvent = syntaxNode is EventFieldDeclarationSyntax;


            foreach (var variable in syntaxNode.Declaration.Variables)
            {
                modelType.Add(new FieldModel
                {
                    AccessModifier = accessModifier,
                    Modifier = modifier,
                    IsEvent = isEvent,
                    Type = typeName,
                    Name = variable.Identifier.ToString(),
                    ContainingTypeName = containingClass
                });
            }

            return modelType;
        }
    }
}
