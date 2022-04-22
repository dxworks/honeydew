using Honeydew.Extractors.CSharp.Visitors;
using Honeydew.Models.CSharp;
using Honeydew.Models.Types;
using HoneydewCore.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Honeydew.Extractors.CSharp.Visitors.Utils.CSharpExtractionHelperMethods;

namespace Honeydew.Extractors.CSharp.Visitors.Concrete;

public class FieldInfoVisitor : ICSharpFieldVisitor
{
    public IList<IFieldType> Visit(BaseFieldDeclarationSyntax syntaxNode, SemanticModel semanticModel,
        IList<IFieldType> modelType)
    {
        var allModifiers = syntaxNode.Modifiers.ToString();
        var accessModifier = CSharpConstants.DefaultFieldAccessModifier;
        var modifier = allModifiers;

        CSharpConstants.SetModifiers(allModifiers, ref accessModifier, ref modifier);

        var typeName =
            GetFullName(syntaxNode.Declaration, semanticModel, out var isNullable);

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
                IsNullable = isNullable
            });
        }

        return modelType;
    }
}
