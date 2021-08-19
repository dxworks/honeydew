using System.Collections.Generic;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Property
{
    public class PropertyInfoVisitor : ExtractionVisitor<CSharpSyntacticModel, CSharpSemanticModel>,
        ICSharpPropertyVisitor
    {
        public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, IPropertyType modelType)
        {
            var allModifiers = syntaxNode.Modifiers.ToString();
            var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
            var modifier = allModifiers;

            var containingClass = "";
            if (syntaxNode.Parent is BaseTypeDeclarationSyntax classDeclarationSyntax)
            {
                containingClass = InheritedSemanticModel.GetFullName(classDeclarationSyntax);
            }

            CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

            var typeName = InheritedSemanticModel.GetFullName(syntaxNode.Type);

            modifier = InheritedSyntacticModel.SetTypeModifier(syntaxNode.Type.ToString(), modifier);

            var accessors = new List<string>();

            if (syntaxNode.AccessorList != null)
            {
                foreach (var accessor in syntaxNode.AccessorList.Accessors)
                {
                    var accessorModifiers = accessor.Modifiers.ToString();
                    var accessorKeyword = accessor.Keyword.ToString();

                    if (string.IsNullOrEmpty(accessorModifiers))
                    {
                        accessors.Add(accessorKeyword);
                    }
                    else
                    {
                        accessors.Add(accessorModifiers + " " + accessorKeyword);
                    }
                }
            }

            var isEvent = false;
            var name = "";
            switch (syntaxNode)
            {
                case EventDeclarationSyntax eventDeclarationSyntax:
                    isEvent = true;
                    name = eventDeclarationSyntax.Identifier.ToString();
                    break;
                case PropertyDeclarationSyntax propertyDeclarationSyntax:
                    name = propertyDeclarationSyntax.Identifier.ToString();
                    break;
            }

            modelType.AccessModifier = accessModifier;
            modelType.Modifier = modifier;

            modelType.IsEvent = isEvent;
            modelType.Type = typeName;
            modelType.Name = name;
            modelType.ContainingTypeName = containingClass;
            modelType.Accessors = accessors;
            modelType.CyclomaticComplexity = InheritedSyntacticModel.CalculateCyclomaticComplexity(syntaxNode);

            return modelType;
        }
    }
}
