using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewExtractors.CSharp.Metrics.Extraction;
using HoneydewExtractors.CSharp.Utils;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.Core.Metrics.Extraction.Property
{
    public class PropertyInfoVisitor : IRequireCSharpExtractionHelperMethodsVisitor, ICSharpPropertyVisitor
    {
        public CSharpExtractionHelperMethods CSharpHelperMethods { get; set; }

        public void Accept(IVisitor visitor)
        {
        }

        public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, IPropertyType modelType)
        {
            var allModifiers = syntaxNode.Modifiers.ToString();
            var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
            var modifier = allModifiers;

            var containingClass = "";
            var classDeclarationSyntax = syntaxNode.Parent as BaseTypeDeclarationSyntax;
            if (classDeclarationSyntax != null)
            {
                containingClass = CSharpHelperMethods.GetFullName(classDeclarationSyntax);
            }

            CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

            if (classDeclarationSyntax is InterfaceDeclarationSyntax && string.IsNullOrEmpty(modifier))
            {
                modifier = CSharpConstants.AbstractIdentifier;
            }

            var typeName = CSharpHelperMethods.GetFullName(syntaxNode.Type);

            modifier = CSharpHelperMethods.SetTypeModifier(syntaxNode.Type.ToString(), modifier);

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
            modelType.CyclomaticComplexity = CSharpHelperMethods.CalculateCyclomaticComplexity(syntaxNode);

            return modelType;
        }
    }
}
