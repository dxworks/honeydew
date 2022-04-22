using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Models.Types;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class PropertyInfoVisitor : ICSharpPropertyVisitor
{
    public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        var allModifiers = syntaxNode.Modifiers.ToString();
        var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
        var modifier = allModifiers;

        var classDeclarationSyntax = syntaxNode.Parent as BaseTypeDeclarationSyntax;

        CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

        if (classDeclarationSyntax is InterfaceDeclarationSyntax && string.IsNullOrEmpty(modifier))
        {
            modifier = CSharpConstants.AbstractIdentifier;
        }

        var typeName = GetFullName(syntaxNode.Type, semanticModel, out var isNullable);

        modifier = SetTypeModifier(syntaxNode.Type.ToString(), modifier);

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
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);
        modelType.IsNullable = isNullable;

        return modelType;
    }
}
