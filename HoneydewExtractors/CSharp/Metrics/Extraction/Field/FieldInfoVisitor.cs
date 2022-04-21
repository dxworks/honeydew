using System.Collections.Generic;
using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Fields;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HoneydewExtractors.CSharp.Metrics.Extraction.CSharpExtractionHelperMethods;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Field;

public class FieldInfoVisitor : ICSharpFieldVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

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
