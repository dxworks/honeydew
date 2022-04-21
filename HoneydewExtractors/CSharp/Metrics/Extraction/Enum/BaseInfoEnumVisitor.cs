using HoneydewCore.Utils;
using HoneydewExtractors.Core.Metrics.Visitors;
using HoneydewExtractors.Core.Metrics.Visitors.Classes;
using HoneydewModels.CSharp;
using HoneydewModels.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static HoneydewExtractors.CSharp.Metrics.Extraction.CSharpExtractionHelperMethods;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.Enum;

public class BaseInfoEnumVisitor : ICSharpEnumVisitor
{
    public void Accept(IVisitor visitor)
    {
    }

    public IEnumType Visit(EnumDeclarationSyntax syntaxNode, SemanticModel semanticModel, IEnumType modelType)
    {
        var accessModifier = CSharpConstants.DefaultClassAccessModifier;
        var modifier = "";
        CSharpConstants.SetModifiers(syntaxNode.Modifiers.ToString(), ref accessModifier, ref modifier);

        modelType.Name = GetFullName(syntaxNode, semanticModel).Name;
        modelType.AccessModifier = accessModifier;
        modelType.Modifier = modifier;
        modelType.ClassType = "enum";
        modelType.ContainingNamespaceName = GetContainingNamespaceName(syntaxNode, semanticModel);
        modelType.ContainingClassName = GetContainingClassName(syntaxNode, semanticModel);
        modelType.Type = "int";
        var baseTypeSyntax = syntaxNode.BaseList?.Types.FirstOrDefault();
        if (baseTypeSyntax != null)
        {
            modelType.Type = baseTypeSyntax.ToString();
        }

        modelType.BaseTypes.Add(new BaseTypeModel
        {
            Type = new EntityTypeModel
            {
                Name = "System.Enum",
                FullType = new GenericType
                {
                    Name = "System.Enum"
                }
            },
            Kind = "class"
        });

        return modelType;
    }
}
