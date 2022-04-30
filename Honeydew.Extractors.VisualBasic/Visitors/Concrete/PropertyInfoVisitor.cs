using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class PropertyInfoVisitor : IExtractionVisitor<PropertyStatementSyntax, SemanticModel, IPropertyType>
{
    public IPropertyType Visit(PropertyStatementSyntax syntaxNode, SemanticModel semanticModel,
        IPropertyType modelType)
    {
        var allModifiers = syntaxNode.Modifiers.ToString();
        var accessModifier = VisualBasicConstants.DefaultFieldAccessModifier;
        var modifier = allModifiers;

        VisualBasicConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

        var typeName = GetFullName(syntaxNode, semanticModel, out var isNullable);

        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;

        modelType.Type = typeName;
        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.CyclomaticComplexity = CalculateCyclomaticComplexity(syntaxNode);
        modelType.IsNullable = isNullable;

        return modelType;
    }
}
