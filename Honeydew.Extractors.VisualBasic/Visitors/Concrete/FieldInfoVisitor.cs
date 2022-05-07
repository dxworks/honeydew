using Honeydew.Extractors.Dotnet;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Honeydew.Models.VisualBasic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using static Honeydew.Extractors.VisualBasic.Visitors.Utils.VisualBasicExtractionHelperMethods;

namespace Honeydew.Extractors.VisualBasic.Visitors.Concrete;

public class FieldInfoVisitor : IExtractionVisitor<ModifiedIdentifierSyntax, SemanticModel, IFieldType>
{
    public IFieldType Visit(ModifiedIdentifierSyntax syntaxNode, SemanticModel semanticModel, IFieldType modelType)
    {
        var fieldDeclarationSyntax = syntaxNode.GetParentDeclarationSyntax<FieldDeclarationSyntax>();
        var variableDeclaratorSyntax = syntaxNode.GetParentDeclarationSyntax<VariableDeclaratorSyntax>();

        if (fieldDeclarationSyntax is null || variableDeclaratorSyntax is null)
        {
            return modelType;
        }

        var allModifiers = fieldDeclarationSyntax.Modifiers.ToString();
        var accessModifier = VisualBasicConstants.DefaultFieldAccessModifier;
        var modifier = allModifiers;

        VisualBasicConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

        var typeName = GetFullName(variableDeclaratorSyntax, semanticModel, out var isNullable);

        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.Type = typeName;
        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.IsNullable = isNullable;

        return modelType;
    }
}
