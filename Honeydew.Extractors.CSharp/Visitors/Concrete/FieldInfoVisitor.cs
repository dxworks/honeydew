using Honeydew.Extractors.CSharp.Utils;
using Honeydew.Extractors.CSharp.Visitors.Utils;
using Honeydew.Extractors.Visitors;
using Honeydew.Models.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class FieldInfoVisitor : IExtractionVisitor<VariableDeclaratorSyntax, SemanticModel, IFieldType>
{
    public IFieldType Visit(VariableDeclaratorSyntax syntaxNode, SemanticModel semanticModel, IFieldType modelType)
    {
        var declaration = syntaxNode.GetParentDeclarationSyntax<BaseFieldDeclarationSyntax>();

        if (declaration is null)
        {
            return modelType;
        }

        var allModifiers = declaration.Modifiers.ToString();
        var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
        var modifier = allModifiers;

        CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

        var typeName = GetFullName(declaration, semanticModel, out var isNullable);

        var isEvent = declaration is EventFieldDeclarationSyntax;

        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.IsEvent = isEvent;
        modelType.Type = typeName;
        modelType.Name = syntaxNode.Identifier.ToString();
        modelType.IsNullable = isNullable;

        return modelType;
    }
}
