﻿using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Properties;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Property;

public class PropertyInfoVisitor : ICSharpPropertyVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IPropertyType Visit(BasePropertyDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        var allModifiers = syntaxNode.Modifiers.ToString();
        var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
        var modifier = allModifiers;

        var containingClass = "";
        var classDeclarationSyntax = syntaxNode.Parent as BaseTypeDeclarationSyntax;
        if (classDeclarationSyntax != null)
        {
            containingClass = CSharpExtractionHelperMethods.GetFullName(classDeclarationSyntax, semanticModel).Name;
        }

        CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

        if (classDeclarationSyntax is InterfaceDeclarationSyntax && string.IsNullOrEmpty(modifier))
        {
            modifier = CSharpConstants.AbstractIdentifier;
        }

        var typeName = CSharpExtractionHelperMethods.GetFullName(syntaxNode.Type, semanticModel, out var isNullable);

        modifier = CSharpExtractionHelperMethods.SetTypeModifier(syntaxNode.Type.ToString(), modifier);

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
        modelType.CyclomaticComplexity = CSharpExtractionHelperMethods.CalculateCyclomaticComplexity(syntaxNode);
        modelType.IsNullable = isNullable;

        return modelType;
    }
}
